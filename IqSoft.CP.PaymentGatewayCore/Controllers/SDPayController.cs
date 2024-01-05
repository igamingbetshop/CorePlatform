using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.PaymentGateway.Models.SDPay;
using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Web;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class SDPayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "109.75.47.208",
            "52.79.105.217",
            "18.179.125.225",
            "13.250.128.86",
            "52.221.132.138",
            "13.125.133.85"
        };

        [HttpPost]
        [Route("{partnerId}/api/SDPayP2P/P2PApiRequest")]
        public ActionResult P2PApiRequest(ResultInput input, int partnerId)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.SDPayP2P);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                return ApiRequest(input, partnerId, paymentSystem.Id);
            }
            catch
            {
                var response = new ResultOutput
                {
                    Command = "60071"
                };
                response.ResponseCode = (int)SDPayHelpers.GetErrorCode(Constants.Errors.GeneralException);
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SDPayQuickPay/QuickPayP2PApiRequest")]
        public ActionResult QuickPayP2PApiRequest(ResultInput input, int partnerId)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.SDPayQuickPay);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                return ApiRequest(input, partnerId, paymentSystem.Id);
            }
            catch
            {
                var response = new ResultOutput
                {
                    Command = "60071"
                };
                response.ResponseCode = (int)SDPayHelpers.GetErrorCode(Constants.Errors.GeneralException);

                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SDPay/PayoutResult")]
        public ActionResult PayoutRequest(PayoutInput input, int partnerId)
        {
            var response = "<span id=\"resultLable\">{0}</span>";
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var transferInfomation = HttpUtility.UrlDecode(input.HiddenField1);

                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(clientBl);
                using var notificationBl = new NotificationBll(clientBl);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.SDPayQuickPay);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);

                var key1 = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SDPayWithdrawKey1).StringValue;
                var key2 = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SDPayWithdrawKey2).StringValue;

                var result = DecryptData(transferInfomation, key1, key2);
                var orderInput = new TransferInfomation();
                try
                {
                    orderInput = SerializeAndDeserialize.XmlDeserializeFromString<TransferInfomation>(result);
                }
                catch
                {
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                }
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(orderInput.SerialNumber));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != "CNY")
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystem.Id,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                if (request.Amount != Convert.ToDecimal(orderInput.IntoAmount))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);

                if (orderInput.RecordsState == 2)//success
                {
                    request.ExternalTransactionId = orderInput.Id.ToString();
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved,
                        string.Empty, request.CashDeskId, null, true, request.Parameters, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                    response = string.Format(response, "Success");
                }
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = string.Format(response, "Error");
                Program.DbLogger.Error(new Exception(ex.Detail.Message));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = string.Format(response, "Error");
            }
            return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
        }

        private static string DecryptData(string data, string key1, string key2)
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(key1);
                byte[] keyIV = Convert.FromBase64String(key2);
                byte[] inputByteArray = Convert.FromBase64String(data);
                using var des = DES.Create();
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, des.CreateDecryptor(keyBytes, keyIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                var res = Encoding.UTF8.GetString(mStream.ToArray());
                return res.Substring(0, res.Length - 32);
            }
            catch
            {
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);

            }
        }

        private ActionResult ApiRequest(ResultInput input, int partnerId, int paymentSystemId)
        {
            var response = new ResultOutput
            {
                Command = "60071"
            };
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var partnerBl = new PartnerBll(paymentSystemBl);
                var key1 = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.SDPayKey1);
                var key2 = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.SDPayKey2);
                var result = DecryptData(input.res, key1, key2);
                var orderInput = new OrderInput();
                try
                {
                    var point = "</message>";
                    result = result.Substring(0, result.IndexOf(point) + point.Length);
                    orderInput = SerializeAndDeserialize.XmlDeserializeFromString<OrderInput>(result);
                }
                catch
                {
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                }
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(orderInput.TransactionId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                response.TransactionId = request.Id.ToString();
                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                response.ClientId = client.Id.ToString();
                if (client.CurrencyId != "CNY")
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                var merchantId = partnerPaymentSetting.UserName;
                if (merchantId != orderInput.MerchantId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                response.MerchantId = merchantId;
                if (request.Amount != orderInput.Amount)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                if (orderInput.ResponseCode == 1)
                {
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    response.ResponseCode = (int)SDPayHelpers.ResponseCodes.Success;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    response.ResponseCode = (int)SDPayHelpers.ResponseCodes.Success;
                else
                    response.ResponseCode = SDPayHelpers.GetErrorCode(ex.Detail.Id);
            }
            catch (Exception)
            {
                response.ResponseCode = SDPayHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
        }
    }
}
