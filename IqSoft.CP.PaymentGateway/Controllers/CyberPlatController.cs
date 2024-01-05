using System;
using System.Net.Http;
using System.ServiceModel;
using System.Collections.Generic;
using System.Web.Http;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.BLL.Caching;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.CyberPlat;
using IqSoft.CP.DAL.Models.Cache;
using System.Linq;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class CyberPlatController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.CyberPlat);
        private readonly static int paymentSystemId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CyberPlat).Id;
        [HttpGet]
        [Route("api/CyberPlat/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri]InputBase input)
        {
            HttpResponseMessage resp;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                switch (input.Action)
                {
                    case CyberPlatHelpers.Methods.Check:
                        resp = CommonFunctions.ConvertObjectToXml(Check(input), Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                        break;
                    case CyberPlatHelpers.Methods.Payment:
                        resp = CommonFunctions.ConvertObjectToXml(Payment(input), Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                        break;
                    case CyberPlatHelpers.Methods.Status:
                        resp = CommonFunctions.ConvertObjectToXml(Status(input), Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                        break;
                    case CyberPlatHelpers.Methods.Cancel:
                        resp = CommonFunctions.ConvertObjectToXml(Cancel(input), Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                        break;
                    default:
                        var response = new ResponseBase { Code = (int)CyberPlatHelpers.ResponseCodes.WrongAction };
                        resp = CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                        break;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                ex.Detail.Id == Constants.Errors.RequestAlreadyPayed ||
                                ex.Detail.Id == Constants.Errors.PaymentRequestAlreadyExists))
                {
                    var response = new Response
                    {
                        Code = (int)CyberPlatHelpers.ResponseCodes.Success,
                        Message = CyberPlatHelpers.Statuses.Success,
                        Date = ex.Detail.DateTimeInfo ?? DateTime.UtcNow
                    };
                    resp = CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                    WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(resp));
                }
                else
                {
                    var response = ex.Detail == null
                        ? new ResponseBase
                        {
                            Code = CyberPlatHelpers.GetErrorCode(Constants.Errors.GeneralException),
                            Message = ex.Message
                        }
                        : new ResponseBase
                        {
                            Code = CyberPlatHelpers.GetErrorCode(ex.Detail.Id),
                            Message = ex.Detail.Message
                        };
                    resp = CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
                    WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(resp));
                }
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    Code = CyberPlatHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                resp = CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
            }
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(resp.Content));
            return resp;
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

                if(clientBll.GetClientPaymentSettings(client.Id, (int)ClientPaymentStates.Blocked, false).Any(x=>x.PartnerPaymentSettingId==partnerPaymentSetting.Id))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);

                return new ResponseBase { Code = (int)CyberPlatHelpers.ResponseCodes.Success, Message = CyberPlatHelpers.Statuses.Success };
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
                Amount = input.Amount / 0.95m,
                ClientId = clientId,
                CurrencyId = client.CurrencyId,
                Info = input.Additional,
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
                        Code = (int)CyberPlatHelpers.ResponseCodes.Success,
                        Message = CyberPlatHelpers.Statuses.Success,
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
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CyberPlat);
                        if (paymentSystem == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                        var paymentRequest = paymentSystemBl.GetPaymentRequest(input.Receipt, paymentSystem.Id, (int)PaymentRequestTypes.Deposit);

                        var document = documentBl.GetDocumentFromDb((int)paymentRequest.PartnerPaymentSettingId, input.Receipt,
                                                                    (int)OperationTypes.Deposit);


                        documentBl.RollBackPaymentRequest(new List<Document> { document });
                        return new Response { Code = (int)CyberPlatHelpers.ResponseCodes.Success, Message = CyberPlatHelpers.Statuses.Success };
                    }
                }
            }
        }
    }
}