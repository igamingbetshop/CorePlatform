using System;
using System.ServiceModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.PaymentGateway.Models.Kassa24;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class Kassa24Controller : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "88.204.242.62",
            "89.218.238.146",
            "2.135.241.202",
            "176.108.65.31",
            "92.47.15.162"
        };
        private readonly static int paymentSystemId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Kassa24).Id;
        [HttpGet]
        [Route("api/Kassa24/ApiRequest")]
        public ActionResult ApiRequest([FromQuery]InputBase input)
        {
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                switch (input.Action)
                {
                    case Kassa24Helpers.Methods.Check:
                        return Ok(CommonFunctions.ConvertObjectToXml(Check(input), Constants.HttpContentTypes.ApplicationXml));
                    case Kassa24Helpers.Methods.Payment:
                        return Ok(CommonFunctions.ConvertObjectToXml(Payment(input), Constants.HttpContentTypes.ApplicationXml));
                    case Kassa24Helpers.Methods.Status:
                        return Ok(CommonFunctions.ConvertObjectToXml(Status(input), Constants.HttpContentTypes.ApplicationXml));
                    case Kassa24Helpers.Methods.Cancel:
                        return Ok(CommonFunctions.ConvertObjectToXml(Cancel(input), Constants.HttpContentTypes.ApplicationXml));
                    default:
                        var response = new ResponseBase { Code = (int)Kassa24Helpers.ResponseCodes.WrongAction };
                        return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
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
                    return Ok(CommonFunctions.ConvertObjectToXml(resp, Constants.HttpContentTypes.ApplicationXml));
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    Code = Kassa24Helpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
        }

        private static ResponseBase Check(InputBase input)
        {
            using (var clientBll = new ClientBll(new DAL.Models.SessionIdentity(), Program.DbLogger))
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

        private static Response Payment(InputBase input)
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

            using (var paymentSystemBl = new PaymentSystemBll(new DAL.Models.SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest);
                        PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                        scope.Complete();
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

        private static Response Status(InputBase input)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new DAL.Models.SessionIdentity(), Program.DbLogger))
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

        private static Response Cancel(InputBase input)
        {
            using (var clientBl = new ClientBll(new DAL.Models.SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        var paymentRequest = paymentSystemBl.GetPaymentRequest(input.Receipt, paymentSystemId, (int)PaymentRequestTypes.Deposit);

                        var document = documentBl.GetDocumentFromDb((int)paymentRequest.PartnerPaymentSettingId, input.Receipt,
                                                                    (int)OperationTypes.TransferFromPaymentSystemToClient);


                        documentBl.RollBackPaymentRequest(new List<Document> { document });
                        return new Response { Code = (int)Kassa24Helpers.ResponseCodes.Success, Message = Kassa24Helpers.Statuses.Success };
                    }
                }
            }
        }
    }
}