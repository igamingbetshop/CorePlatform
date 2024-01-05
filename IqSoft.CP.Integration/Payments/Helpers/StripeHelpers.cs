using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Stripe;
using log4net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class StripeHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(input.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																				   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.StripeUrl).StringValue;
				var partner = CacheManager.GetPartnerById(client.PartnerId);
				var header = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } };
				var postData = CommonFunctions.GetUriEndocingFromObject(new 
				{
					id = input.Id,
					name = partner.Name
				});
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = header,
					Url = $"{url}/products",
					PostData = postData
				};
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var product = JsonConvert.DeserializeObject<ProductOutput>(res);

			    postData = CommonFunctions.GetUriEndocingFromObject(new
				{
					currency = input.CurrencyId,
					product = product.Id,					
					unit_amount = input.Amount * 100
				});
				httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = header,
					Url = $"{url}/prices",
					PostData = postData
				};
				res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var price = JsonConvert.DeserializeObject<ProductOutput>(res);
				var metadata = input.Id.ToString();
				httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = header,
				    Url = $"{url}/payment_links",
					PostData = $"line_items[0][quantity]=1&line_items[0][price]={price.Id}&metadata[payment_request_id]={metadata}&after_completion[type]=redirect&after_completion[redirect][url]={cashierPageUrl}"
				};
				res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var response = JsonConvert.DeserializeObject<PaymentLinkOutput>(res);

				return response.Url;
			}
		}
	}
}
