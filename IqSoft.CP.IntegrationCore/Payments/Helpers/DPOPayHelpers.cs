using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using System;
using IqSoft.CP.Integration.Payments.Models.DPOPay;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class DPOPayHelpers
    {
        public static string CallDPOPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DPOPayApiUrl).StringValue;
                var paymentGatewayUrl = string.Format("{0}api/DPOPay/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue);
                var redirectData = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(new
                {
                    PaymentGatewayUrl = paymentGatewayUrl,
                    CashierPageUrl = cashierPageUrl
                }));
                var distributionUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                var paymentRequestInput = new PaymentInput
                {
                    CompanyToken = partnerPaymentSetting.Password,
                    Request = "createToken",
                    Transaction = new TransactionInput
                    {
                        PaymentAmount = Math.Round(input.Amount, 2),
                        PaymentCurrency = input.CurrencyId,
                        CompanyRef = input.Id.ToString(),
                        RedirectURL = string.Format("{0}/redirect/rp?rd={1}", distributionUrl, redirectData),
                        BackURL = cashierPageUrl,
                        CompanyRefUnique = 1
                    },
                    Services = new List<Service> { new Service
                    {
                        ServiceDescription = partner.Name,
                        ServiceType = partnerPaymentSetting.UserName,
                        ServiceDate = DateTime.UtcNow.ToString("yyyy/MM/dd HH:MM")
                    } }
                };

                var xml = SerializeAndDeserialize.SerializeToXml(paymentRequestInput, "API3G");
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationXml,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/API/v6/", url),
                    PostData = xml
                };
                var deserializer = new XmlSerializer(typeof(PaymentOutput), new XmlRootAttribute("API3G"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                {
                    var response = (PaymentOutput)deserializer.Deserialize(stream);
                    if (response.Result == "000")
                    {
                        input.ExternalTransactionId = response.TransToken;
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return string.Format("{0}/payv2.php?ID={1}", url, response.TransToken);
                    }

                    throw new Exception(response.ResultExplanation);
                }
            }
        }

        public static void GetPaymentRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            using var notificationBl = new NotificationBll(clientBl);
            var client = CacheManager.GetClientById(paymentRequest.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DPOPayApiUrl).StringValue;
            var verifyTokenInput = new VerifyInput
            {
                CompanyToken = partnerPaymentSetting.Password,
                Request = "verifyToken",
                TransactionToken = paymentRequest.ExternalTransactionId
            };
            var xml = SerializeAndDeserialize.SerializeToXml(verifyTokenInput, "API3G");
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationXml,
                RequestMethod = HttpMethod.Post,
                Url = string.Format("{0}/API/v6/", url),
                PostData = xml
            };
            var deserializer = new XmlSerializer(typeof(VerifyOutput), new XmlRootAttribute("API3G"));
            using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
            {
                var verifyOutput = (VerifyOutput)deserializer.Deserialize(stream);
                if (verifyOutput.Result == "000")
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                else if (verifyOutput.Result == "901" || verifyOutput.Result == "904")
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, verifyOutput.ResultExplanation, notificationBl);
            }
        }
    }
}