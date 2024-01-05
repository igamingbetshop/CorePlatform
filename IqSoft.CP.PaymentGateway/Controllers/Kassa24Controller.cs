using System;
using System.Net.Http;
using System.ServiceModel;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using System.Linq;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.PaymentGateway.Models.Kassa24;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class Kassa24Controller : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Kassa24);
        private readonly static int paymentSystemId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Kassa24).Id;

        [HttpGet]
        [Route("api/Kassa24/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri]InputBase input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                switch (input.Action)
                {
                    case Kassa24Helpers.Methods.Check:
                        return CommonFunctions.ConvertObjectToXml(Check(input), Constants.HttpContentTypes.ApplicationXml);
                    case Kassa24Helpers.Methods.Payment:
                        return CommonFunctions.ConvertObjectToXml(Payment(input), Constants.HttpContentTypes.ApplicationXml);
                    case Kassa24Helpers.Methods.Status:
                        return CommonFunctions.ConvertObjectToXml(Status(input), Constants.HttpContentTypes.ApplicationXml);
                    case Kassa24Helpers.Methods.Cancel:
                        return CommonFunctions.ConvertObjectToXml(Cancel(input), Constants.HttpContentTypes.ApplicationXml);
                    default:
                        var response = new ResponseBase { Code = (int)Kassa24Helpers.ResponseCodes.WrongAction };
                        return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                ex.Detail.Id == Constants.Errors.RequestAlreadyPayed ||
                                ex.Detail.Id == Constants.Errors.PaymentRequestAlreadyExists))
                {
                    var resp = new Response
                    {
                        Code = (int)Kassa24Helpers.ResponseCodes.Success,
                        Message = Kassa24Helpers.Statuses.Success,
                        Date = ex.Detail.DateTimeInfo ?? DateTime.UtcNow
                    };
                    return CommonFunctions.ConvertObjectToXml(resp, Constants.HttpContentTypes.ApplicationXml);
                }
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        Code = Kassa24Helpers.GetErrorCode(Constants.Errors.GeneralException),
                        Message = ex.Message
                    }
                    : new ResponseBase
                    {
                        Code = Kassa24Helpers.GetErrorCode(ex.Detail.Id),
                        Message = ex.Detail.Message
                    };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    Code = Kassa24Helpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        private ResponseBase Check(InputBase input)
        {
            using (var clientBll = new ClientBll(new DAL.Models.SessionIdentity(), WebApiApplication.DbLogger))
            {
                int clientId;
                if (!int.TryParse(input.Number, out clientId))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongConvertion);

                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);

                if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);

                if (clientBll.GetClientPaymentSettings(client.Id, (int)ClientPaymentStates.Blocked, false).Any(x => x.PartnerPaymentSettingId == partnerPaymentSetting.Id))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);

                if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);
                return new ResponseBase { Code = (int)Kassa24Helpers.ResponseCodes.Success, Message = Kassa24Helpers.Statuses.Success };
            }
        }

        private Response Payment(InputBase input)
        {
            int clientId;
            Response response;
            if (!int.TryParse(input.Number, out clientId))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongConvertion);
            if (input.Amount <= 0)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
            var clientState = client.State;
            if (clientState == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
            var paymentRequest = new PaymentRequest
            {
                Amount = input.Amount,// + input.Commission,
                ClientId = clientId,
                CurrencyId = client.CurrencyId,
                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                ExternalTransactionId = input.Receipt
            };

            using (var paymentSystemBl = new PaymentSystemBll(new DAL.Models.SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info);
                        PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                        scope.Complete();
                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
                        BaseHelpers.BroadcastDepositLimit(info);
                        response = new Response
                        {
                            Code = (int)CyberPlatHelpers.ResponseCodes.Success,
                            Message = CyberPlatHelpers.Statuses.Success,
                            Date = request.CreationTime
                        };
                    }
                }
            }
            return response;
        }

        private Response Status(InputBase input)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new DAL.Models.SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequest(input.Receipt, paymentSystemId, (int)PaymentRequestTypes.Deposit);

                    if (paymentRequest == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    return new Response
                    {
                        Code = (int)Kassa24Helpers.ResponseCodes.Success,
                        Message = Kassa24Helpers.Statuses.Success,
                        Date = paymentRequest.CreationTime
                    };
                }
            }

        }

        private Response Cancel(InputBase input)
        {
            using (var clientBl = new ClientBll(new DAL.Models.SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        var paymentRequest = paymentSystemBl.GetPaymentRequest(input.Receipt, paymentSystemId, (int)PaymentRequestTypes.Deposit);

                        var document = documentBl.GetDocumentFromDb((int)paymentRequest.PartnerPaymentSettingId, input.Receipt,
                                                                    (int)OperationTypes.Deposit);


                        documentBl.RollBackPaymentRequest(new List<Document> { document });
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                        return new Response { Code = (int)Kassa24Helpers.ResponseCodes.Success, Message = Kassa24Helpers.Statuses.Success };
                    }
                }
            }
        }
    }
}