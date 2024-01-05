﻿using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Pay3000;
using log4net;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class Pay3000Helpers
	{
		private static readonly BllPaymentSystem PaymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Pay3000);

		public static ClientPaymentInfo RegisterConsent(ClientPaymentInfo input, int partnerId, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId);
			var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay3000Url).StringValue;
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, PaymentSystem.Id,
																			   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
			if (string.IsNullOrEmpty(cashierPageUrl))
				cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
			else
				cashierPageUrl = string.Format(cashierPageUrl, session.Domain);
			var postData = JsonConvert.SerializeObject(new
			{
				acceptUrl = cashierPageUrl,
				rejectUrl = cashierPageUrl,
				failureUrl = cashierPageUrl
			});
			var headers = new Dictionary<string, string>
					{
						{ "Authorization", $"Bearer {partnerPaymentSetting.Password}"}
					};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{baseUrl}ecommerce/register-consent?ownerAccountNumber={input.WalletNumber}",
				PostData = postData
			};
			var info = new ClientPaymentInfo();
			try
			{

				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<ConsentOutput>(res);
				using (var clientBl = new ClientBll(new SessionIdentity(), log))
				{
					info = clientBl.GetClientPaymentAccountDetails(client.Id, PaymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												.FirstOrDefault(x => x.WalletNumber == output.ConsentId);
					if (info == null)
					{
						info = PaymentHelpers.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
						{
							AccountNickName = PaymentSystem.Name,
							ClientFullName = input.WalletNumber,
							Type = (int)ClientPaymentInfoTypes.Wallet,
							ClientId = client.Id,
							PartnerPaymentSystemId = partnerPaymentSetting.Id,
							WalletNumber = output.ConsentId,
							CardExpireDate = output.Expires,
							State = (int)ClientPaymentInfoStates.Pending
						}, log);
					}
					info.State = (int)ClientPaymentInfoStates.Pending;
				}
			}
			catch (Exception ex)
			{
				var error = JsonConvert.DeserializeObject<ConsentErrorOutput>(ex.Message);
				if (error.ErrorCode == "EC01")
					throw new Exception("Customer not found!");
			}
			return info;
		}

		public static void PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId);
			InitiatePayment(input, cashierPageUrl, "DEBIT_CUSTOMER", client, session, log);
		}

		public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId);
			var cashierPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
			if (string.IsNullOrEmpty(cashierPageUrl))
				cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
			else
				cashierPageUrl = string.Format(cashierPageUrl, session.Domain);
			InitiatePayment(input, cashierPageUrl, "CREDIT_CUSTOMER", client, session, log);
			return new PaymentResponse
			{
				Status = PaymentRequestStates.PayPanding,
			};
		}

		public static void InitiatePayment(PaymentRequest input, string cashierPageUrl, string type, BllClient client, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var clientBl = new ClientBll(paymentSystemBl))
				{
					var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay3000Url).StringValue;
					var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																					   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
					var clientWalletInfo = clientBl.GetClientPaymentAccountDetails(client.Id, partnerPaymentSetting.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												.FirstOrDefault();
					if (clientWalletInfo == null)
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
					var postData = JsonConvert.SerializeObject(new
					{
						acceptUrl = cashierPageUrl,
						rejectUrl = cashierPageUrl,
						failureUrl = cashierPageUrl
					});
					var headers = new Dictionary<string, string>
					{
						{ "Authorization", $"Bearer {partnerPaymentSetting.Password}"},
						{ "consent-id", clientWalletInfo.WalletNumber}
					};
					var httpRequestInput = new HttpRequestInput
					{
						ContentType = Constants.HttpContentTypes.ApplicationJson,
						RequestMethod = Constants.HttpRequestMethods.Post,
						RequestHeaders = headers,
						Url = $"{baseUrl}ecommerce/initiate-payment?amount={input.Amount}&type={type}",
						PostData = postData
					};
					var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
					if (res.Contains("error"))
						throw new Exception(res);
					var output = JsonConvert.DeserializeObject<PaymentOutput>(res);
					input.ExternalTransactionId = output.PaymentRequestId;
					paymentSystemBl.ChangePaymentRequestDetails(input);
					using (var notificationBl = new NotificationBll(clientBl))
					{
						clientBl.ChangeDepositRequestState(input.Id, PaymentRequestStates.PayPanding, string.Empty, notificationBl);
					}
				}
			}
		}

		public static string OKTOAccountRegistration(int clientId)
		{
			var client = CacheManager.GetClientById(clientId);
			var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay3000Url).StringValue;
			var postData = JsonConvert.SerializeObject(new
			{
				externalReferenceId = client.Id.ToString(),
				email = client.Email,
				mobileNo = client.MobileNumber,
				language = "English",
				kycLevelReq = "DEFAULT"
			});

			var headers = new Dictionary<string, string>
					{
						{ "Authorization", "Bearer " + "G8HqfDNz9bcf5svXE2G9gCCnfOE" }
					};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = "https://sit.oktopay.eu/api/integration/remote-registration/initiate?" + CommonFunctions.GetUriDataFromObject(postData)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			if (!res.Contains("Error"))
			{
				var output = JsonConvert.DeserializeObject<RegistrationOutput>(res);
			}
			else
				throw new Exception();
			return "";
		}

		public static string OKTOKYCProcess(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId);
			var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay3000Url).StringValue;
			var postData = JsonConvert.SerializeObject(new
			{
				birthDate = client.BirthDate.ToString(),
				email = client.Email,
				firstName = client.FirstName,
				lastName = client.LastName,
				nationality = client.Citizenship,
				residenceCountry = client.Citizenship,
			});

			var headers = new Dictionary<string, string>
					{
						{ "Authorization", "Bearer " + "G8HqfDNz9bcf5svXE2G9gCCnfOE" }
					};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = "https://sit.oktopay.eu/api/integration/remote-registration/initiate-kyc",
				PostData = postData
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			if (!res.Contains("Error"))
			{
				var output = JsonConvert.DeserializeObject<RegistrationOutput>(res);
			}
			else
				throw new Exception();
			return "";
		}

		public static void CancelPaymentRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			if (paymentRequest.LastUpdateTime.AddMinutes(5) < DateTime.UtcNow)
			{
				using (var clientBl = new ClientBll(session, log))
				{
					using (var documentBll = new DocumentBll(clientBl))
					{
						using (var notificationBl = new NotificationBll(documentBll))
						{
							if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
							{
								clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.CanceledByClient, string.Empty, notificationBl);
							}
							else
							{
								clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.CanceledByClient,
																	  string.Empty, null, null, false, string.Empty, documentBll, notificationBl);
							}
						}
					}
				}
			}
		}
	}
}