using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Huch;
using log4net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class HuchHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var partner = CacheManager.GetPartnerById(client.PartnerId);
			if (string.IsNullOrEmpty(client.Email))
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.HuchAuthenticationUrl).StringValue;
			var data = new
			{
				grant_type = "client_credentials",
				client_id = partnerPaymentSetting.UserName,
				client_secret = partnerPaymentSetting.Password.Split(',')[0]
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = url,
				PostData = CommonFunctions.GetUriDataFromObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var auth = JsonConvert.DeserializeObject<AuthOutput>(res);
			url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.HuchUrl).StringValue;
			var paymentData = new
			{
				merchant_id = partnerPaymentSetting.Password.Split(',')[1],
				currency = client.CurrencyId,
				amount = input.Amount * 100,
				success_url = cashierPageUrl,
				pending_bank_confirmation_url = cashierPageUrl,
				decline_url = cashierPageUrl,
				order_ref = input.Id.ToString(),
				customer_email = client.Email,
				shop_name = partner.Name
			};
			var headers = new Dictionary<string, string> { { "Authorization", "Bearer " + auth.access_token } };
			httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = url,
				PostData = JsonConvert.SerializeObject(paymentData)
			};
			res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var paymentOuthput = JsonConvert.DeserializeObject<PaymentOutput>(res);
			return paymentOuthput.url;
		}

		public static void GetPaymentRequestStatus(PaymentRequest input, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.HuchAuthenticationUrl).StringValue;
			var data = new
			{
				grant_type = "client_credentials",
				client_id = partnerPaymentSetting.UserName,
				client_secret = partnerPaymentSetting.Password.Split(',')[0]
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = url,
				PostData = CommonFunctions.GetUriDataFromObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var auth = JsonConvert.DeserializeObject<AuthOutput>(res);
			url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.HuchUrl).StringValue;	
			httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + auth.access_token } },
				Url = $"{url}{input.ExternalTransactionId}"
			};
			res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var paymentOuthput = JsonConvert.DeserializeObject<PaymentOutput>(res);
			using (var clientBl = new ClientBll(session, log))
			using (var paymentSystemBl = new PaymentSystemBll(clientBl))
			using (var notificationBl = new NotificationBll(clientBl))
			{
				if (paymentOuthput.payment_status == "PAID_RECEIVED" || paymentOuthput.payment_status == "PAID")
					clientBl.ApproveDepositFromPaymentSystem(input, false);
				else if (paymentOuthput.payment_status == "EXPIRED" || paymentOuthput.payment_status == "CANCELLED" ||
						 paymentOuthput.payment_status == "FAILED" || paymentOuthput.payment_status == "AUTH_FAILED" || paymentOuthput.payment_status == "EXECUTE_FAILED")
					clientBl.ChangeDepositRequestState(input.Id, PaymentRequestStates.Deleted, paymentOuthput.payment_status, notificationBl);
			}
		}
	}
}
