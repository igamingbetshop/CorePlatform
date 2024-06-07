using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Xprizo;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class XprizoHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.XprizoUrl).StringValue;
			var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
			var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
			var data = new PaymentInput
			{
				reference = input.Id.ToString(),
				amount = input.Amount
			};
			switch (paymentSystem.Name)
			{
				case "XprizoCard":
					data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					data.customer = client.Id.ToString();
					data.currencyCode = client.CurrencyId;
					data.redirect = cashierPageUrl;
					url = $"{url}/Transaction/CardDeposit";
					break;
				case "XprizoWallet":
					data.fromAccountId = Convert.ToInt64(paymentInfo.CardNumber);
					data.toAccountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					url = $"{url}/Transaction/RequestPayment";
					break;
				case "XprizoMpesa":
					data.mobileNumber = paymentInfo.MobileNumber;
					data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					//data.description = "Pass";
					url = $"{url}/Transaction/MPesaDeposit";
					break;
				case "XprizoUPI":
					data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					url = $"{url}/Transaction/UpiDeposit";
					break;
				default: 
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
			}
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.Password.Split(',')[0] },
					                                              { "x-api-version", partnerPaymentSetting.Password.Split(',')[1] } },
				Url = url,
				PostData = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
			};
			log.Info(JsonConvert.SerializeObject(data));
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PaymentOutput>(response);

			switch (paymentSystem.Name)
			{
				case "XprizoCard":
					return output.value;
				case "XprizoWallet":
					return "ConfirmationCode";
				case "XprizoMpesa":
					if(output.status == "Pending")
						return "ConfirmationCode";
					else if(output.status == "Active")
						return null;
					else 
						throw new Exception($"Error: {output.status} {output.value} ");
				case "XprizoUPI":
					return output.value;
				default:
					throw new Exception($"Error: {response} ");
			};
		}


		public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.XprizoUrl).StringValue;
				var url = $"{baseUrl}/Transaction/SendPayment?action=1";
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
				var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
				var data = new PaymentInput
				{
					reference = paymentRequest.Id.ToString(),
					amount = paymentRequest.Amount
				};
				switch (paymentSystem.Name)
				{
					case "XprizoWallet":
						data.toAccountId = Convert.ToInt64(paymentInfo.WalletNumber);
						data.fromAccountId = Convert.ToInt64(partnerPaymentSetting.UserName); 
						break;
					case "XprizoMpesa":
						data.mobileNumber = paymentInfo.MobileNumber;
						data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
						break;
					default:
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
				}
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.Password.Split(',')[0] },
																    { "x-api-version", partnerPaymentSetting.Password.Split(',')[1] } },
					Url = url,
					PostData = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
				};
				log.Info(JsonConvert.SerializeObject(data));
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
				paymentRequest.ExternalTransactionId = output.id.ToString();
				paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
				return new PaymentResponse
				{
					Status = PaymentRequestStates.Approved,
				};
				throw new Exception();
			}
		}
	}
}
