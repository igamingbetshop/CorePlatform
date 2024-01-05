using System;
using Newtonsoft.Json;
using log4net;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Integration.Payments.Models.Skrill;
using System.IO;
using System.Xml.Serialization;
using System.Net;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class SkrillHelpers
    {
        public static string CallSkrillApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var paymentRequestInput = new SkrillRequestInput
            {
                PayToEmail = partnerPaymentSetting.UserName,
                Amount = input.Amount,
                Currency = input.CurrencyId,
                RecipientDescription = partner.Name,
                TransactionId = input.Id,
                ReturnUrl = cashierPageUrl,
                ReturnUrlText = partner.Name,
                return_url_target = (int)ReturnUrlTarget._parent,
                CancelUrl = cashierPageUrl,
                CancelUrlTarget = (int)ReturnUrlTarget._parent,
                StatusUrl = string.Format("{0}/api/Skrill/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                Language = session.LanguageId ?? Constants.DefaultLanguageId,
                PrepareOnly = 1
            };
            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.SkrillDepositUrl).StringValue;

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            return string.Format("{0}/?sid={1}", url, CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }

        public static PaymentResponse SendPaymentRequestToSkrill(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (string.IsNullOrEmpty(partnerPaymentSetting.UserName) || string.IsNullOrEmpty(partnerPaymentSetting.Password))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);
                var key = CommonFunctions.ComputeMd5(partnerPaymentSetting.Password).ToLower();
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var prepareInput = new PrepareInput
                {
                    action = SkrillActions.Prepare,
                    email = partnerPaymentSetting.UserName,
                    password = key,
                    amount = amount,
                    currency = paymentRequest.CurrencyId,
                    subject = partner.Name,
                    note = partner.Name,
                    bnf_email = paymentInfo.Email,
                    frn_trn_id = paymentRequest.Id.ToString()
                };
                var prepareOutput = SkrillAction(prepareInput);
                if (prepareOutput.Sid == null)
                    throw new Exception(prepareOutput.ErrorObj.ErrorMessage);
                paymentRequest.ExternalTransactionId = prepareOutput.Sid;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                var transferInput = new TransferInput { action = SkrillActions.Transfer, sid = prepareOutput.Sid };
                var transferOutput = SkrillAction(transferInput);
                if (transferOutput.TransactionData.Status == 2)
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved
                    };
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                    Description = transferOutput.TransactionData.StatusMessage
                };
            }
        }

        private static TransactionOutput SkrillAction(object input)
        {
            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.SkrillWithdrawUrl).StringValue;
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                Url = string.Format("{0}?{1}", url, CommonFunctions.GetUriEndocingFromObject(input))
            };
            var deserializer = new XmlSerializer(typeof(TransactionOutput), new XmlRootAttribute("response"));
            using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
            {
                return (TransactionOutput)deserializer.Deserialize(stream);
            }
        }    
    }
}
