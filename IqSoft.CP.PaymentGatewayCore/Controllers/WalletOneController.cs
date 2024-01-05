using System;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.ServiceModel;
using System.Security.Cryptography;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.PaymentGateway.Models.WalletOne;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class WalletOneController : ControllerBase
    {
        [HttpPost]
        [Route("api/WalletOne/ApiRequest")]
        public ActionResult ApiRequest(WalletOneRequestStatus input)
        {
            string response;
            using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(paymentSystemBl);
            Program.DbLogger.Info(JsonConvert.SerializeObject(input));
            var request = paymentSystemBl.GetPaymentRequestById(input.WMI_PAYMENT_NO);
            if (request == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
            try
            {
                Program.DbLogger.Info("WalletOne provider type = " + input.WMI_PAYMENT_TYPE);
                Program.DbLogger.Info("WMI_INVOICE_OPERATIONS = " + input.WMI_INVOICE_OPERATIONS);
                var client = CacheManager.GetClientById(request.ClientId);
                var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.WalletOneSecretKey).StringValue;

                var sign = input.WMI_SIGNATURE;
                input.WMI_SIGNATURE = string.Empty;

                Byte[] bytes = Encoding.GetEncoding("windows-1251").GetBytes(CommonFunctions.GetSortedValuesAsString(input) + secretKey);
                using var md5 = MD5.Create();
                Byte[] hash = md5.ComputeHash(bytes);

                var signature = Convert.ToBase64String(hash);

                if (signature != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                if (input.WMI_ORDER_STATE.ToUpper() == "ACCEPTED")
                {
                    request.ExternalTransactionId = input.WMI_ORDER_ID.ToString();
                    if (input.WMI_PAYMENT_TYPE.ToLower().Contains("creditcard"))
                    {
                        var paymentSystem = CacheManager.GetPaymentSystemByName("CreditCards");
                        request.PaymentSystemId = paymentSystem.Id;
                    }
                    paymentSystemBl.ChangePaymentRequestDetails(request);

                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    return Ok(new StringContent("WMI_RESULT=OK", Encoding.UTF8));
                }
                else
                    return Ok(new StringContent("WMI_RESULT=RETRY", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    return Ok(new StringContent("WMI_RESULT=OK", Encoding.UTF8));
                }
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                response = "WMI_RESULT=RETRY&WMI_DESCRIPTION=" + ex.Message;
                return Ok(new StringContent(response, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = "WMI_RESULT=RETRY&WMI_DESCRIPTION=" + ex.Message;

                return Ok(new StringContent(response, Encoding.UTF8));
            }
        }
    }
}