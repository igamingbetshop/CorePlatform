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
	public static class XprizoHelpers
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
				case Constants.PaymentSystems.XprizoCard:
					data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					data.customer = client.Id.ToString();
					data.currencyCode = client.CurrencyId;
					data.redirect = cashierPageUrl;
					url = $"{url}/Transaction/CardDeposit";
					break;
				case Constants.PaymentSystems.XprizoWallet:
					data.fromAccountId = Convert.ToInt64(paymentInfo.CardNumber);
					data.toAccountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					url = $"{url}/Transaction/RequestPayment";
					break;
				case Constants.PaymentSystems.XprizoMpesa:
					data.mobileNumber = paymentInfo.MobileNumber;
					data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					//data.description = "Pass";
					url = $"{url}/Transaction/MPesaDeposit";
					break;
				case Constants.PaymentSystems.XprizoUPI:
					data.accountId = Convert.ToInt64(partnerPaymentSetting.UserName);
					url = $"{url}/Transaction/UpiDeposit";
					break;
				default:
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
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
				case Constants.PaymentSystems.XprizoCard:
					return output.value;
				case Constants.PaymentSystems.XprizoWallet:
					return "ConfirmationCode";
				case Constants.PaymentSystems.XprizoMpesa:
					if (output.status == "Pending")
						return "ConfirmationCode";
					else if (output.status == "Active")
						return null;
					else
						throw new Exception($"Error: {output.status} {output.value} ");
				case Constants.PaymentSystems.XprizoUPI:
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
				if (!Int64.TryParse(partnerPaymentSetting.UserName, out long accountId))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongApiCredentials);

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
					case Constants.PaymentSystems.XprizoWallet:
						if (!Int64.TryParse(paymentInfo.WalletNumber, out long walletNamber))
						{
							using (var clientBl = new ClientBll(paymentSystemBl))
							using (var documentBl = new DocumentBll(paymentSystemBl))
							using (var notificationBl = new NotificationBll(paymentSystemBl))
							{
								clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, nameof(Constants.Errors.WrongInputParameters),
																	null, null, false, paymentRequest.Parameters, documentBl, notificationBl);

								throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
							}
						}
						data.toAccountId = walletNamber;
						data.fromAccountId = accountId;
						break;
					case Constants.PaymentSystems.XprizoMpesa:
						data.mobileNumber = paymentInfo.MobileNumber;
						data.accountId = accountId;
						break;
					default:
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
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
				try
				{
					var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
					var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
					paymentRequest.ExternalTransactionId = output.id.ToString();
					paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
					return new PaymentResponse
					{
						Status = PaymentRequestStates.Approved,
						Description = output.description
					};
				}
				catch (Exception ex)
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					using (var documentBl = new DocumentBll(paymentSystemBl))
					using (var notificationBl = new NotificationBll(paymentSystemBl))
					{
						clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, ex.Message,
															null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Failed,
                        };
                    }
				}
			}
		}
	}
}
