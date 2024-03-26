using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Yaspa;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class YaspaHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.YaspaUrl).StringValue;
			var errorPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentErrorPageUrl).StringValue;
			var data = new
			{
				customerIdentifier = client.Id.ToString(),
				paymentGiro = "SEPA",
				amount = input.Amount.ToString(),
				currency = client.CurrencyId,
				reference = input.Id.ToString(),
				journeyType = "HOSTED_VERIFIED_PAYMENT",
				successRedirectUrl = cashierPageUrl,
				failureRedirectUrl = errorPageUrl,
				successBankRedirectUrl = cashierPageUrl,
				failureBankRedirectUrl = errorPageUrl
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "AuthorizationCitizen", partnerPaymentSetting.Password} },
				Url = $"{url}/v2/payins/hosted-payin/generate-link",
				PostData = JsonConvert.SerializeObject(data)
			};
			log.Info(JsonConvert.SerializeObject(data));
			try
			{
				CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			}
			catch (Exception ex)
			{
				var output = JsonConvert.DeserializeObject<PaymentOutput>(ex.Message);
				if (output.StatusCode == "308")
					return output.Message;
				throw new Exception(output.Message);
			}
			return null;
		}
	}
}
