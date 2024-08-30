using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Katarun;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class KatarunHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KatarunUrl).StringValue;
			var data = new PaymentInput
			{
				reference = input.Id.ToString(),
				brand_id = partnerPaymentSetting.UserName,
				client = new Models.Katarun.Client
				{
					email = client.Email
				},
				purchase = new Purchase
				{
					products = new List<Models.Katarun.Product>
					 {
						  new Models.Katarun.Product
						  {
							  name = "Deposit",
							  price = Convert.ToInt32(input.Amount * 100)
						  }
					 }
				},
				success_redirect = cashierPageUrl,
				failure_redirect = cashierPageUrl,
				cancel_redirect = cashierPageUrl
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
			    Url = $"{url}purchases/",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PaymentOutput>(res);			
			return output.checkout_url;
		}

		public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);

				var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.KatarunUrl);
				var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var data = new PayoutInput
				{
					reference = paymentRequest.Id.ToString(),
					brand_id = partnerPaymentSetting.UserName,
					client = new PayoutClient
					{
						email = client.Email,
						phone = client.MobileNumber
					},
					payment = new Payment
					{
						amount = (amount * 100).ToString(),
						currency = client.CurrencyId
					},
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
					Url = $"{url}payouts/",
					PostData = JsonConvert.SerializeObject(data)
				};
				log.Info(JsonConvert.SerializeObject(httpRequestInput));
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				log.Info(response);
				var payoutResponse = JsonConvert.DeserializeObject<PayoutOutput>(response);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var payoutData = new
				{
					beneficiaryReference = paymentInfo.BankCode,
					beneficiaryName = paymentInfo.BankBranchName,
					beneficiaryAccount = paymentInfo.BankAccountNumber
				};
				httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
					Url = payoutResponse.execution_url,
					PostData = JsonConvert.SerializeObject(payoutData)
				};
				log.Info(JsonConvert.SerializeObject(httpRequestInput));
				response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				log.Info(response);
				return new PaymentResponse
				{
					Status = PaymentRequestStates.PayPanding,
				};
			}
		}
	}
}
