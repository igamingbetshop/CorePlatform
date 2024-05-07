using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.QuikiPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class QuikiPayHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			if (string.IsNullOrEmpty(client.Email))
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.QuikiPayUrl).StringValue;
			var errorPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentErrorPageUrl).StringValue;
			var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
			var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
			if (distributionUrlKey == null || distributionUrlKey.Id == 0)
				distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
			var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
			var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId).Name;
			var isCrypto = paymentsystem.Contains("Crypto");
			var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
			var data = new PaymentInput
			{
				merchant = partnerPaymentSetting.UserName,
				customer_name = client.Id.ToString(),
				customer_email = client.Email,
				currency = client.CurrencyId,
				order_id = input.Id.ToString(),
				amount = input.Amount.ToString(),
				success_url = $"{distributionUrl}/redirect/RedirectRequest?redirectUrl={cashierPageUrl}",
				cancel_url = $"{distributionUrl}/redirect/RedirectRequest?redirectUrl={errorPageUrl}",
				callback_url = $"{paymentGateway}/api/QuikiPay/ApiRequest",
				products_data = "Deposit",
				code = isCrypto ? "C-01" : null
			};
			var signature = GetObjectPropertyValuesAsString(data, typeof(PaymentInput));
			data.signature = CommonFunctions.ComputeSha256(signature + partnerPaymentSetting.Password);
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = $"{url}/api/v1.1/payment/checkout",
				PostData = JsonConvert.SerializeObject(data)
			};
			log.Info(JsonConvert.SerializeObject(data));
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
			if (!string.IsNullOrEmpty(output.PaymentUrl))
				return output.PaymentUrl;
			else
				throw new Exception($"Error: ");
		}


		public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				if (string.IsNullOrEmpty(client.Email))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.QuikiPayUrl).StringValue;
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);				
				var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
				var payoutInput = new PayoutInput
				{
					type = "payout",
					source = "crypto",
					currency = client.CurrencyId,
					amount = paymentRequest.Amount.ToString(),
					end_user_ip = paymentInfo.TransactionIp,
					crypto_currency = paymentInfo.AccountType,
					crypto_address = paymentInfo.WalletNumber,
					withdrawal_id = paymentRequest.Id.ToString(),
					customer_name = client.Id.ToString(),
					customer_email = client.Email,
					is_local = "1",
					auto = 0,
					callback_url = $"{paymentGateway}/api/QuikiPay/PayoutRequest",
				};
				var signature = GetObjectPropertyValuesAsString(payoutInput, typeof(PayoutInput));
				payoutInput.signature = CommonFunctions.ComputeSha256(signature + partnerPaymentSetting.Password);
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Api-Key", partnerPaymentSetting.UserName } },
					Url = $"{url}/api/v1.1/withdrawal",
					PostData = JsonConvert.SerializeObject(payoutInput)
				};
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var paymentOutput = JsonConvert.DeserializeObject<PayoutOutput>(res);
				if (paymentOutput.success)
					return new PaymentResponse
					{
						Status = PaymentRequestStates.PayPanding,
					};
				throw new Exception($"Error Code: {paymentOutput.code}, Message: {paymentOutput.message}");
			}
		}

		public static string GetObjectPropertyValuesAsString(object obj, Type type)
		{
			var concatenatedString = new StringBuilder();
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				var value = property.GetValue(obj);
				if (value != null)
				{
					concatenatedString.Append(value);
				}
			}

			return concatenatedString.ToString();
		}
	}
}
