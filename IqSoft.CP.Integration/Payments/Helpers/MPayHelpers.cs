using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.MPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class MPayHelpers
	{
		private static Dictionary<string, string> MPayURLs { get; set; } = new Dictionary<string, string>
		{
			{ Constants.PaymentSystems.MPayEFT, "eft" },
			{ Constants.PaymentSystems.MPayPapara, "papara" },
			{ Constants.PaymentSystems.MPayCreditCard, "cc" },
			{ Constants.PaymentSystems.MPayVIPCreditCard, "vipcc" },
			{ Constants.PaymentSystems.MPayPayfix, "payfix" },
			{ Constants.PaymentSystems.MPayMefete, "mefete" },
			{ Constants.PaymentSystems.MPayParazula, "parazula" },
			{ Constants.PaymentSystems.MPayQR, "qr" },
			{ Constants.PaymentSystems.MPayPopy, "popy" },
			{ Constants.PaymentSystems.MPayPayco, "payco" },
			{ Constants.PaymentSystems.MPayFastHavale, "fasthavale" }
		};
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			var client = CacheManager.GetClientById(input.ClientId.Value);
			var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
			if (string.IsNullOrEmpty(client.FirstName?.Trim()))
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
			if (string.IsNullOrEmpty(client.LastName?.Trim()))
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
			if (paymentSystem.Name == Constants.PaymentSystems.MPayCreditCard && string.IsNullOrEmpty(client.BirthDate.ToString()))
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidBirthDate);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MPayUrl).StringValue;
			var postData = new PaymentInput
			{
				data = new Data
				{
					trx = input.Id.ToString(),
					return_url = cashierPageUrl,
					amount = input.Amount
				},
				user = new Models.MPay.User
				{
					userID = client.Id.ToString(),
					username = client.UserName,
					fullname = $"{client.FirstName} {client.LastName}",
					yearofbirth = paymentSystem.Name == Constants.PaymentSystems.MPayCreditCard ? client.BirthDate.ToString() : null
				}
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
				Url = $"{url}/deposit/{MPayURLs[paymentSystem.Name]}",
				PostData = JsonConvert.SerializeObject(postData, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PaymentOutput>(res);
			if (output.status == "success")
			{
				var data = JsonConvert.DeserializeObject<DataOutput>(JsonConvert.SerializeObject(output.data));
				return data.url;
			}
			throw new Exception(output.message);
		}


		public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MPayUrl).StringValue;
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var postData = new PaymentInput
				{
					data = new Data
					{
						trx = paymentRequest.Id.ToString(),
						amount = amount
					},
					user = new Models.MPay.User
					{
						userID = client.Id.ToString(),
						username = client.UserName,
						fullname = $"{client.FirstName} {client.LastName}"
					}
				};
				switch (paymentSystem.Name)
				{
					case Constants.PaymentSystems.MPayEFT:
						var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
						if (bankInfo == null)
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
						postData.data.bank_id = bankInfo.BankCode;
						postData.data.account_holders_name = paymentInfo.BankAccountHolder;
						postData.data.iban = paymentInfo.BankAccountNumber;
						postData.data.account_number = paymentInfo.CardNumber;
						break;
					case Constants.PaymentSystems.MPayPapara:
						postData.data.account_no = paymentInfo.WalletNumber;
						break;
					case Constants.PaymentSystems.MPayPayco:
						postData.data.account_number = paymentInfo.WalletNumber;
						break;
					case Constants.PaymentSystems.MPayPayfix:
						postData.data.wallet_id = paymentInfo.AccountNumber;
						postData.data.wallet_holders_name = paymentInfo.DocumentId;
						break;
					case Constants.PaymentSystems.MPayMefete:
						postData.data.account_number = paymentInfo.AccountNumber;
						postData.user.tckn = paymentInfo.DocumentId;
						break;
					case Constants.PaymentSystems.MPayParazula:
						postData.data.account_number = paymentInfo.AccountNumber;
						postData.user.tckn = paymentInfo.DocumentId;
						break;
					case Constants.PaymentSystems.MPayPopy:
						postData.data.popyIBAN = paymentInfo.WalletNumber;
						break;
					default:
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);				
				}
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
					Url = $"{url}/withdraw/{MPayURLs[paymentSystem.Name]}",
					PostData = JsonConvert.SerializeObject(postData, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
				};
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var payoutResponse = JsonConvert.DeserializeObject<PaymentOutput>(response);
				if (payoutResponse.status == "success")
					return new PaymentResponse
					{
						Status = PaymentRequestStates.PayPanding,
					};
				throw new Exception(payoutResponse.message);
			}
		}
	}
}

