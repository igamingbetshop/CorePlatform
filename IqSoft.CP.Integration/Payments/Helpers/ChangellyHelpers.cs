using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.Changelly;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Ocsp;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class ChangellyHelpers
    {
        public static string CallChangellyApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var receiptWallet = NOWPayHelpers.GetTransactionWalletNumber(input.Id, client.Id, input.Amount, paymentInfo.Info, log);
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                var paymentInput = new
                {
                    from = client.CurrencyId,
                    fromDefault = client.CurrencyId,
                    to = paymentInfo.Info,
                    toDefault = paymentInfo.Info,
                    amount = input.Amount.ToString("F"),
                    address = receiptWallet,
                    merchant_id = partnerPaymentSetting.UserName,
                    payment_id = input.Id.ToString()
                };

                var data = AESEncryptHelper.EncryptDistributionString(CommonFunctions.GetUriEndocingFromObject(paymentInput));
                return string.Format("{0}/changelly/paymentprocessing?data={1}", distributionUrl, data);
            }
        }

        public static string GenerateSignature(string body, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                byte[] dataToSign = Encoding.UTF8.GetBytes(body);
                byte[] signatureBytes = rsa.SignData(dataToSign, new SHA256CryptoServiceProvider());

                return Convert.ToBase64String(signatureBytes);
            }
        }
    }
}
