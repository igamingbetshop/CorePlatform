using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.PaymentGateway.Models.CartToCard;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class CardToCardController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "88.99.175.216"
        };

        [HttpGet]
        [Route("api/CardToCard/ApiRequest")]
        public ActionResult ApiRequest(int paymentRequestId)
        {
            var response = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var partnerBl = new PartnerBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                            if (request == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId);
                            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CardToCard);
                            var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.CardToCardApiUrl);
                            var verifyInput = new
                            {
                                payment_id = request.Id
                            };
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = HttpMethod.Post,
                                Url = string.Format("{0}/c2c/verifyPay", url),
                                PostData = CommonFunctions.GetUriEndocingFromObject(verifyInput)
                            };
                            var res = JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                            if (res.Code == 0)
                            {
                                request.ExternalTransactionId = string.Format("{0}_{0}", request.Id);
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                            }
                            else
                                throw new Exception(res.Message);
                            return Ok(new StringContent("OK", Encoding.UTF8));
                            
                        }
                    }
                }
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
