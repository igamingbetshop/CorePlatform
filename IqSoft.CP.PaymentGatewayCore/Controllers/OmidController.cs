using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.PaymentGateway.Models.Omid;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class OmidController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "?"//cup distribution ip
        };

        [HttpGet]
        [Route("api/Omid/ApiRequest")]
        public ActionResult ApiRequest(int paymentRequestId)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var partnerBl = new PartnerBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OmidApiUrl).StringValue;
                var amount = request.Amount - (request.CommissionAmount ?? 0);
                var paymentRequestInput = new
                {
                    mid = partnerPaymentSetting.UserName,
                    amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianRial, Convert.ToInt32(amount))),
                    authority = request.ExternalTransactionId
                };
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = HttpMethod.Get,
                    Url = string.Format("{0}/trs/webservice/verifyRequest?params={1}", url, JsonConvert.SerializeObject(paymentRequestInput))
                };
                var result = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                if (result.Error && result.Result == request.ExternalTransactionId)
                {
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                }
                else
                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, result.Message, null, null, false,
                                                        string.Empty, documentBl, notificationBl);

                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = ex.Message;
            }
            return Ok(new StringContent(response, Encoding.UTF8));
          
        }
    }
}
