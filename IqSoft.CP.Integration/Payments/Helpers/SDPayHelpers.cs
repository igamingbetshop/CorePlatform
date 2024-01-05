using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.SDPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    class SDPayHelpers
    {
        public static class CommandCodes
        {
            public const int PC = 6006;
            public const int Mobile = 6010;
        }

        public static string CallSDPayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrWhiteSpace(input.Info))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

            using (var partnerBl = new PartnerBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.Renminbi)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.Renminbi, partnerPaymentSetting);
                    amount *= rate;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.Renminbi);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                }
                var merchantId = partnerPaymentSetting.UserName;
                var orderData = new OrderData
                {
                    MerchantId = merchantId,
                    Language = CommonHelpers.LanguageISOCodes[session.LanguageId],
                    User = new UserInfo
                    {
                        OrderId = input.Id.ToString(),
                        ClientId = client.Id.ToString(),
                        Amount = Math.Round(amount, 2),
                        Currency = 1,   // only CNY 
                        Remark = partner.Name,
                        Time = DateTime.Now.ToString(),
                        BackurlBrowser = session.Domain
                    }
                };

                var url = string.Empty;
                var gatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                if (session.DeviceType == (int)DeviceTypes.Mobile)
                {
                    if (paymentSystem.Name == Constants.PaymentSystems.SDPayP2P)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ImpermissiblePaymentSetting);
                    orderData.Command = CommandCodes.Mobile.ToString();
                    orderData.User.BackUrl = string.Format("{0}/{1}/{2}", gatewayUrl,
                                                               client.PartnerId, "api/SDPayQuickPay/QuickPayP2PApiRequest");
                    url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayMobileUrl).StringValue;

                }
                else
                {
                    if (paymentSystem.Name == Constants.PaymentSystems.SDPayP2P)
                        orderData.User.BackUrl = string.Format("{0}/{1}/{2}", gatewayUrl,
                                                               client.PartnerId, "api/SDPayP2P/P2PApiRequest");
                    else
                        orderData.User.BackUrl = string.Format("{0}/{1}/{2}", gatewayUrl,
                                                               client.PartnerId, "api/SDPayQuickPay/QuickPayP2PApiRequest");
                    orderData.Command = CommandCodes.PC.ToString();
                    url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayDesktopUrl).StringValue;
                }
                var orderXml = SerializeAndDeserialize.SerializeToXml(orderData, "message");
                var key1 = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayKey1).StringValue;
                var key2 = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayKey2).StringValue;
                var MD5Key = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayMD5Key).StringValue;
                string md5Hash = CommonFunctions.ComputeMd5(orderXml + MD5Key);
                var des = EncryptData(orderXml + md5Hash + CommonFunctions.ComputeMd5(GetMac() + DateTime.Now.ToFileTime()), key1, key2);
                return SubmitRequest(merchantId, des, url);
            }
        }

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.Renminbi)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.Renminbi, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.Renminbi);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayWithdrawUrl).StringValue;
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);

                var transferInfomation = new PayoutRequestInput
                {
                    Id = 0,
                    IntoAccount = paymentInfo.BankAccountNumber,
                    IntoName = paymentInfo.BankName,
                    IntoBank1 = bankInfo.BankCode,
                    IntoBank2 = string.Empty,
                    Amount = Math.Round(amount, 2),
                    SerialNumber = input.Id.ToString()
                };
                var transferInfomationXml = SerializeAndDeserialize.SerializeToXml(transferInfomation, "TransferInfomation");
                var key1 = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayWithdrawKey1).StringValue;
                var key2 = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SDPayWithdrawKey2).StringValue;
                var getFundInfo = EncryptData(transferInfomationXml, key1, key2);
                SDPayCustomer.CustomerSoapClient cn = new SDPayCustomer.CustomerSoapClient();
                int result = cn.GetFund(partnerPaymentSetting.UserName, getFundInfo);
                return new PaymentResponse
                {
                    Status = result == 0 ? PaymentRequestStates.PayPanding : PaymentRequestStates.Failed
                };
            }
        }

        public static string SubmitRequest(string pid, string des, string url)
        {
			if (!string.IsNullOrEmpty(url) && pid.Length > 0 && des.Length > 0)
			{
				return string.Format("<form method='post' action='{0}'>" +
										 "<input type= 'hidden' id='pid' name='pid' value= '{1}' />" +
										 "<input type= 'hidden' id='des' name='des' value= '{2}' />" +
										 "<input type='submit' value='Submit' id='sendtoken'></form>",
										 url, pid, des);
			}
            return string.Empty;
        }

        private static string GetMac()
        {
            var randomValue = new Random();
            return DateTime.Now.ToFileTime().ToString() + randomValue.Next().ToString();
        }

        public static string EncryptData(string data, string key1, string key2)
        {
            byte[] keyBytes = Convert.FromBase64String(key1);//ASCIIEncoding.ASCII.GetBytes(key);
            byte[] keyIV = Convert.FromBase64String(key2);
            byte[] inputByteArray = Encoding.UTF8.GetBytes(data);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(keyBytes, keyIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }
    }
}