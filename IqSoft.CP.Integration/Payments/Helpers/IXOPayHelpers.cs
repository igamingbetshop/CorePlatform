using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.IXOPay;
using System.Security.Cryptography;
using IqSoft.CP.Integration.Payments.Models;
using System.Linq;
using System.Globalization;
using log4net.Plugin;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class IXOPayHelpers
	{
		public static string CallIXOPayApi(PaymentRequest input, string successPageUrl, string errorPageUrl, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var clientBl = new ClientBll(new SessionIdentity(), log))
				{
					var client = CacheManager.GetClientById(input.ClientId.Value);
					var partner = CacheManager.GetPartnerById(client.PartnerId);
					var paymentSystemName = CacheManager.GetPaymentSystemById(input.PaymentSystemId).Name;
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																					   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
					var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.IXOPayApiUrl).StringValue;
					successPageUrl = successPageUrl ?? CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentSuccessPageUrl).StringValue;
					errorPageUrl = errorPageUrl ?? CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentErrorPageUrl).StringValue;
					var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
					var apikeys = partnerPaymentSetting.Password.Split(',');
					var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
					if (string.IsNullOrEmpty(paymentInfo.Country))
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
					ClientPaymentInfo info = null;
					if (paymentSystemName == Constants.PaymentSystems.IXOPayCC)
					{
						if (string.IsNullOrEmpty(paymentInfo.WalletNumber))
							info = null;
						else
						{
							var firstSixDigits = paymentInfo.WalletNumber.Substring(0, 6);
							var lastFourDigits = paymentInfo.WalletNumber.Substring(paymentInfo.WalletNumber.Length - 4, 4);
							info = clientBl.GetClientPaymentAccountDetails($"{firstSixDigits}****{lastFourDigits}", input.PaymentSystemId,
													  new List<int> { (int)ClientPaymentInfoTypes.Wallet }, client.Id).FirstOrDefault();
						}
					}
					else
						info = clientBl.GetClientPaymentAccountDetails(client.Id, input.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												 .FirstOrDefault();
					var iswithRegister = false;
					string reference = null;
					if ((paymentSystemName == Constants.PaymentSystems.IXOPayPayPal || paymentSystemName == Constants.PaymentSystems.IXOPayCC))
					{
						iswithRegister = info == null;
						reference = info != null ? info.WalletNumber : null;
					}
					var paymentInput = JsonConvert.SerializeObject(new
					{
						merchantTransactionId = input.Id.ToString(),
						amount = input.Amount.ToString("F", CultureInfo.InvariantCulture),
						currency = client.CurrencyId,
						description = partner.Name,
						withRegister = iswithRegister,
						successUrl = successPageUrl,
						cancelUrl = errorPageUrl,
						errorUrl = errorPageUrl,
						callbackUrl = string.Format("{0}/api/ixopay/ApiRequest", paymentGateway),
						referenceUuid = reference,
						customer = new
						{
							identification = client.Id.ToString(),
							firstName = client.FirstName,
							lastName = client.LastName,
							company = partner.Name,
							email = client.Email,
							billingCountry = paymentSystemName == Constants.PaymentSystems.IXOPayCC ? null : paymentInfo.Country
						},
						extraData = paymentSystemName != Constants.PaymentSystems.IXOPayPayPal ? null : new
						{
							merchantSessionId = paymentInfo.Info
						}
					}, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
					var sha = CommonFunctions.ComputeSha512(paymentInput);
					var url = $"/api/v3/transaction/{apikeys[0]}/debit";
					var str = $"POST\n{sha}\n{Constants.HttpContentTypes.ApplicationJson}\n{url}";
					var keyByte = Encoding.UTF8.GetBytes(apikeys[1]);

					using (var hmacsha512 = new HMACSHA512(keyByte))
					{
						var msg = Encoding.UTF8.GetBytes(str);
						var hash = hmacsha512.ComputeHash(msg);
						var signature = Convert.ToBase64String(hash);
						var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
						var headers = new Dictionary<string, string>
					{
						{ "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
						{ "X-Signature", signature}
					};
						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationJson,
							RequestMethod = Constants.HttpRequestMethods.Post,
							RequestHeaders = headers,
							Url = $"{baseUrl}{url}",
							PostData = paymentInput
						};
						var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var output = JsonConvert.DeserializeObject<PaymentOutput>(res);						

						if (output.ReturnType == "ERROR")
							throw new Exception(string.Format("ErrorCode: {0}, ErrorMessage: {1}, AdapterMessage: {2}, AdapterCode: {3}", output.Error[0].ErrorCode,
																		 output.Error[0].ErrorMessage, output.Error[0].AdapterMessage, output.Error[0].AdapterCode));
						if (output.ReturnType == "FINISHED")
							return null;
						return output.RedirectUrl;
					}
				}
			}
		}

		public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var clientBll = new ClientBll(paymentSystemBl))
				{
					var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
					var partner = CacheManager.GetPartnerById(client.PartnerId);
					var paymentSystemName = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId).Name;
					var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																					   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
					var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.IXOPayApiUrl).StringValue;
					var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
					var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
					if (string.IsNullOrEmpty(paymentInfo.Country))
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
					var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
					var apikeys = partnerPaymentSetting.Password.Split(',');
					ClientPaymentInfo clientWalletInfo = null;
					if (paymentSystemName == Constants.PaymentSystems.IXOPayCC)
					{
						clientWalletInfo = clientBll.GetClientPaymentAccountDetails(client.Id, paymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false, paymentInfo.WalletNumber)
											.FirstOrDefault();
					}
					else
						clientWalletInfo = clientBll.GetClientPaymentAccountDetails(client.Id, paymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												 .FirstOrDefault();
					if (clientWalletInfo == null)
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AccountNotFound);
					var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
							   JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
					parameters.Add("WalletNumber", clientWalletInfo.WalletNumber);
					paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
					paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
					var payoutInput = JsonConvert.SerializeObject(new PayoutInput
					{
						MerchantTransactionId = paymentRequest.Id.ToString(),
						Amount = paymentRequest.Amount.ToString("F", CultureInfo.InvariantCulture),
						Currency = client.CurrencyId,
						CallbackUrl = string.Format("{0}/api/ixopay/ApiRequest", paymentGateway),
						Description = partner.Name,
						ReferenceUuid = clientWalletInfo.WalletNumber,
						Customer = new Customer
						{
							Identification = client.Id.ToString(),
							FirstName = client.FirstName,
							LastName = client.LastName,
							Email = client.Email,
							IpAddress = session.LoginIp,
							BillingCountry = paymentInfo.Country
						}
					},
					new JsonSerializerSettings()
					{
						NullValueHandling = NullValueHandling.Ignore
					});
					var sha = CommonFunctions.ComputeSha512(payoutInput);
					var url = $"/api/v3/transaction/{apikeys[0]}/payout";
					var concatReslt = $"POST\n{sha}\n{Constants.HttpContentTypes.ApplicationJson}\n{url}";
					var keyByte = Encoding.UTF8.GetBytes(apikeys[1]);
					using (var hmacsha512 = new HMACSHA512(keyByte))
					{
						var msg = Encoding.UTF8.GetBytes(concatReslt);
						var hash = hmacsha512.ComputeHash(msg);
						var signature = Convert.ToBase64String(hash);
						var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
						var headers = new Dictionary<string, string>
						{
							{ "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
							{ "X-Signature", signature }
						};
						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationJson,
							RequestMethod = Constants.HttpRequestMethods.Post,
							RequestHeaders = headers,
							Url = $"{baseUrl}{url}",
							PostData = payoutInput
						};
						var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var paymentOutput = JsonConvert.DeserializeObject<PayoutOutput>(res);

						if (paymentOutput.Success)
							return new PaymentResponse
							{
								Status = PaymentRequestStates.PayPanding,
							};
						throw new Exception(string.Format("ErrorCode: {0}, AdapterCode: {1}, AdapterMessage {2}", paymentOutput.Errors[0].ErrorCode,
												  paymentOutput.Errors[0].AdapterCode, paymentOutput.Errors[0].AdapterMessage));
					}
				}
			}
		}
		public static void DeRegister(PartnerPaymentSetting partnerPaymentSetting, string walletNumber)
		{
			var baseUrl = CacheManager.GetPartnerSettingByKey(partnerPaymentSetting.PartnerId, Constants.PartnerKeys.IXOPayApiUrl).StringValue;
			var apikeys = partnerPaymentSetting.Password.Split(',');
			var paymentInput = JsonConvert.SerializeObject(new
			{
				merchantTransactionId = CommonFunctions.GetRandomString(10),
				referenceUuid = walletNumber,
			});
			var sha = CommonFunctions.ComputeSha512(paymentInput);
			var url = $"/api/v3/transaction/{apikeys[0]}/deregister";
			var str = $"POST\n{sha}\n{Constants.HttpContentTypes.ApplicationJson}\n{url}";
			var keyByte = Encoding.UTF8.GetBytes(apikeys[1]);

			using (var hmacsha512 = new HMACSHA512(keyByte))
			{
				var msg = Encoding.UTF8.GetBytes(str);
				var hash = hmacsha512.ComputeHash(msg);
				var signature = Convert.ToBase64String(hash);
				var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
				var headers = new Dictionary<string, string>
					{
						{ "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
						{ "X-Signature", signature}
					};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = headers,
					Url = $"{baseUrl}{url}",
					PostData = paymentInput
				};
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<PaymentOutput>(res);
				if (output.ReturnType == "ERROR")
					throw new Exception(string.Format("ErrorCode: {0}, ErrorMessage: {1}, AdapterMessage: {2}, AdapterCode: {1}", output.Error[0].ErrorCode,
																 output.Error[0].ErrorMessage, output.Error[0].AdapterMessage, output.Error[0].AdapterCode));
			}
		}
	}
}
