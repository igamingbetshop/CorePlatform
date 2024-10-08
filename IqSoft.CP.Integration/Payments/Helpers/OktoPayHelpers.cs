using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Pay3000;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class OktoPayHelpers
	{
		private static readonly BllPaymentSystem PaymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.OktoPay);

		public static ClientPaymentInfo RegisterConsent(ClientPaymentInfo input, int partnerId, SessionIdentity session, ILog log)
		{
			var info = new ClientPaymentInfo();
			using (var clientBl = new ClientBll(new SessionIdentity(), log))
			{
				try
				{
					var paymentInfo = clientBl.GetClientPaymentAccountDetails(input.WalletNumber, PaymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, null);
					if (paymentInfo.Any())
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientPaymentInfoAlreadyExists);
					var client = CacheManager.GetClientById(input.ClientId);
					var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OktoPayUrl).StringValue;
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, PaymentSystem.Id,
																					   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
					var headers = new Dictionary<string, string>
					{
						{ "Authorization", $"Bearer {partnerPaymentSetting.Password}"}
					};
					var httpRequestInput = new HttpRequestInput
					{
						ContentType = Constants.HttpContentTypes.ApplicationJson,
						RequestMethod = Constants.HttpRequestMethods.Post,
						RequestHeaders = headers,
						Url = $"{baseUrl}ecommerce/register-consent?ownerAccountNumber={input.WalletNumber}"
					};

					var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
					var output = JsonConvert.DeserializeObject<ConsentOutput>(res);
					info = clientBl.GetClientPaymentAccountDetails(client.Id, PaymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												.FirstOrDefault(x => x.WalletNumber == output.ConsentId);
					if (info == null)
					{
						info = clientBl.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
						{
							AccountNickName = input.WalletNumber,
							Type = (int)ClientPaymentInfoTypes.Wallet,
							ClientId = client.Id,
							PartnerPaymentSystemId = partnerPaymentSetting.Id,
							WalletNumber = output.ConsentId,
							CardExpireDate = output.Expires,
							State = (int)ClientPaymentInfoStates.Pending
						}, null, false);
					}
				}
				catch (FaultException<BllFnErrorType> ex)
				{
					throw BaseBll.CreateException(session.LanguageId, ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id);
				}
				catch (Exception ex)
				{
					//var error = JsonConvert.DeserializeObject<ConsentErrorOutput>(ex.Message);
					if (ex.Message.Contains("EC01"))
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AccountNotFound);
				}
			}
			return info;
		}

		public static void RevokeConsent(PartnerPaymentSetting partnerPaymentSetting, string walletNumber)
		{
			var baseUrl = CacheManager.GetPartnerSettingByKey(partnerPaymentSetting.PartnerId, Constants.PartnerKeys.OktoPayUrl).StringValue;
			var headers = new Dictionary<string, string>
					{
						{ "Authorization", $"Bearer {partnerPaymentSetting.Password}"},
						{ "consent-id", walletNumber}
					};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{baseUrl}ecommerce/consent/revoke"
			};
			CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}

		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			return InitiatePayment(input, cashierPageUrl, "DEBIT_CUSTOMER", client, session, log);
		}

		public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
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

		public static string InitiatePayment(PaymentRequest input, string cashierPageUrl, string type, BllClient client, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var clientBl = new ClientBll(paymentSystemBl))
				{
					var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OktoPayUrl).StringValue;
					var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																					   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
					var clientWalletInfo = clientBl.GetClientPaymentAccountDetails(client.Id, partnerPaymentSetting.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												.FirstOrDefault();
					if (clientWalletInfo == null)
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AccountNotFound);
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
						Url = $"{baseUrl}ecommerce/initiate-payment?amount={input.Amount}&type={type}"
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
					return "ConfirmationCode";
				}
			}
		}

		public static string OKTOAccountRegistration(int clientId)
		{
			var client = CacheManager.GetClientById(clientId);
			var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OktoPayUrl).StringValue;
			var postData = new
			{
				externalReferenceId = client.Id.ToString(),
				email = client.Email,
				mobileNo = client.MobileNumber,
				language = "English",
				kycLevelReq = "DEFAULT"
			};

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
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OktoPayUrl).StringValue;
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

		public static List<int> CancelPaymentRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			var userIds = new List<int>();
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
																	  string.Empty, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
							}
						}
					}
				}
			}
			return userIds;
        }
	}
}