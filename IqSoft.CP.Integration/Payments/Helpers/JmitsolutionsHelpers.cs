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
	public class JmitsolutionsHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var partnerBl = new PartnerBll(paymentSystemBl))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						var client = CacheManager.GetClientById(input.ClientId.Value);
						if (string.IsNullOrWhiteSpace(client.MobileNumber))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
						var partner = CacheManager.GetPartnerById(client.PartnerId);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
						var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JmitsolutionsUrl).StringValue;
						var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
						var data = new
						{
							product = "Deposit",
							amount = (int)input.Amount * 100,
							currency = client.CurrencyId,
							redirectSuccessUrl = cashierPageUrl,
							//redirectFailUrl = cashierPageUrl,
							orderNumber = input.Id.ToString(),
							callbackUrl = string.Format("{0}/api/Jmitsolutions/ApiRequest", paymentGateway),
						};

						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationJson,
							RequestMethod = Constants.HttpRequestMethods.Post,
							RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {partnerPaymentSetting.Password}"  } },
							Url = $"{url}/api/v1/payments",
							PostData = JsonConvert.SerializeObject(data)
						};

						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
						if (output.success)
							return output.processingUrl;
						else
							throw new Exception($"Error: {output.result}");
					}
				}
			}
		}


		public static PaymentResponse PayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SantimaPayUrl).StringValue;
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var paymentInput = new
				{
					amount = (int)paymentRequest.Amount * 100,
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
						ip = session.LoginIp
					}
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {partnerPaymentSetting.Password}" } },
					Url = $"{url}/api/v1/payouts",
					PostData = JsonConvert.SerializeObject(paymentInput)
				};
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
				if (output.success)
				{
					return new PaymentResponse
					{
						Status = PaymentRequestStates.Approved,
					};
				}
				else
					throw new Exception($"Error: {output.errors}");
			}
		}
	}
}
