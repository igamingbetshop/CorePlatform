using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.JetonHavale;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	class JetonHavaleHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(input.ClientId.Value);
				if (string.IsNullOrEmpty(client.FirstName))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
				if (string.IsNullOrEmpty(client.LastName))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																				   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JetonHavaleUrl).StringValue;
				var paymentInput = new PaymentInput
				{
					auth = new Auth
					{
						apiKey = partnerPaymentSetting.UserName,
						secKey = partnerPaymentSetting.Password
					},
					customer = new Customer
					{
						id = client.Id.ToString(),
						username = client.FirstName,
						fullName = client.LastName
					},
					transactionId = input.Id.ToString(),
					returnUrl = cashierPageUrl,
					amount = input.Amount
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = $"{url}/api/payment/deposit/with-partner",
					PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
				};
				var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
				input.ExternalTransactionId = paymentOutput.Deposit.Id;
				paymentSystemBl.ChangePaymentRequestDetails(input);

				return $"https://payment.jetonhavale.com/deposit?token={paymentOutput.Token}";
			}
		}


		public static PaymentResponse PayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				if (string.IsNullOrEmpty(client.FirstName))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
				if (string.IsNullOrEmpty(client.LastName))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JetonHavaleUrl).StringValue;
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
				if (bankInfo == null)
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
				var paymentInput = new PaymentInput
				{
					auth = new Auth
					{
						apiKey = partnerPaymentSetting.UserName,
						secKey = partnerPaymentSetting.Password
					},
					customer = new Customer
					{
						id = client.Id.ToString(),
						username = client.UserName,
						fullName = $"{client.FirstName} {client.LastName}"
					},
					transactionId = paymentRequest.Id.ToString(),
					iban = paymentInfo.BankIBAN,
					bank = bankInfo.BankCode,
					amount = paymentRequest.Amount
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = $"{url}/api/payment/withdraw/with-partner",
					PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
				};
				var paymentOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
				paymentRequest.ExternalTransactionId = paymentOutput.Withdraw.Id;

				paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
				return new PaymentResponse
				{
					Status = PaymentRequestStates.PayPanding,
				};
			}
		}
	}
}
