using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Jmitsolutions;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public static class JmitSolutionsHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																			   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JmitSolutionsUrl).StringValue;
			var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
			var data = new
			{
				product = "Deposit",
				amount = (int)input.Amount * 100,
				currency = client.CurrencyId,
				redirectSuccessUrl = cashierPageUrl,
				redirectFailUrl = cashierPageUrl,
				orderNumber = input.Id.ToString(),
				callbackUrl = $"{paymentGateway}/api/JmitSolutions/ApiRequest",
				locale = session.LanguageId
			};

			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {partnerPaymentSetting.Password}" } },
				Url = $"{url}/api/v1/payments",
				PostData = JsonConvert.SerializeObject(data)
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
			if (!string.IsNullOrEmpty(output.Token))
				using (var paymentSystemBl = new PaymentSystemBll(session, log))
				{
					input.ExternalTransactionId =  output.Token;
					paymentSystemBl.ChangePaymentRequestDetails(input);
				}
			if (output.Success)
				return output.ProcessingUrl;
			throw new Exception($"Error: {response}");
		}

		public static PaymentResponse PayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
			if (string.IsNullOrEmpty(client.Email))
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.EmailCantBeEmpty);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																			   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JmitSolutionsUrl).StringValue;
			var amount = (int)(paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0))*100;
			var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);

			var payoutInput = new
			{
				amount,
				currency = client.CurrencyId,
				orderNumber = paymentRequest.Id.ToString(),
				card = new
				{
					pan = paymentInfo.CardNumber,
					expires = paymentInfo.ExpirationDate
				},
				customer = new
				{
					email = client.Email,
					ip = paymentInfo.TransactionIp
				}
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {partnerPaymentSetting.Password}" } },
				Url = $"{url}/api/v1/payouts",
				PostData = JsonConvert.SerializeObject(payoutInput)
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
			if (!string.IsNullOrEmpty(output?.Payout?.Token))
			{
				using (var paymentSystemBl = new PaymentSystemBll(session, log))
				{
					paymentRequest.ExternalTransactionId =  output.Payout.Token;
					paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
				}
			}
			if (output.Success)
				return new PaymentResponse
				{
					Status = PaymentRequestStates.PayPanding,
				};
			throw new Exception($"Error: {response}");
		}
	}
}
