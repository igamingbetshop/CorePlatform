using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.FreeKassa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class FreeKassaController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "136.243.38.147",
            "136.243.38.149",
            "136.243.38.150",
            "136.243.38.151",
            "136.243.38.189",
            "136.243.38.108",
            "168.119.157.136",
            "168.119.60.227",
            "138.201.88.124",
            "178.154.197.79"
        };

        [HttpPost]
        [Route("api/FreeKassa/ApiRequest")]
        public ActionResult ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        var ip = string.Empty;
                        if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                            ip = header.ToString();
                        Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                        var request = paymentSystemBl.GetPaymentRequestById(input.MERCHANT_ORDER_ID);
                        if (request == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var signature = CommonFunctions.ComputeMd5(string.Format("{0}:{1}:{2}:{3}", partnerPaymentSetting.UserName,
                            input.AMOUNT.ToString(".##"), partnerPaymentSetting.Password.Split('/')[1], request.Id));
                        if (signature.ToLower() != input.SIGN.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                        request.ExternalTransactionId = input.intid.ToString();
                        paymentSystemBl.ChangePaymentRequestDetails(request);
                        response = "OK";
                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                    
                        return Ok(new StringContent(response, Encoding.UTF8));
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            response = "OK";
                            return Ok(new StringContent(response, Encoding.UTF8));
                        }
                        var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                        Program.DbLogger.Error(exp);
                        return Conflict(new StringContent(exp.Message, Encoding.UTF8));
                          }
                    catch (Exception ex)
                    {
                        Program.DbLogger.Error(ex);
                        return Conflict(new StringContent(ex.Message, Encoding.UTF8));
                    }
                }
            }
        }
    }
}
