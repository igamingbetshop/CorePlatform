using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Elite;
using Jose;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	class EliteHelpers
	{
		public static string PaymentRequest(PaymentRequest input, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(input.ClientId.Value);
				if (string.IsNullOrEmpty(client.FirstName))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
				if (string.IsNullOrEmpty(client.LastName))
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																				   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
				var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.EliteUrl).StringValue;
				var token = GetToken(baseUrl, partnerPaymentSetting.Password);
		        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
				var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
				if (bankInfo == null)
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
				var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
				string url;
				switch (paymentSystem.Name)
				{

					case Constants.PaymentSystems.EliteBankTransfer:
						url = "/Api/Deposit/DepositBankTransferCreate";
						break;
					case Constants.PaymentSystems.EliteEftFast:
						url = "/Api/Deposit/DepositFastCreate";
						break;
					case Constants.PaymentSystems.ElitePayFix:
						url = "/Api/Deposit/DepositPayFixCreate";
						break;
					case Constants.PaymentSystems.ElitePapara:
						url = "/Api/Deposit/DepositPaparaCreate";
						break;
					case Constants.PaymentSystems.EliteParazula:
						url = "/Api/Deposit/DepositParazulaCreate";
						break;
					default:
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
				}
				var paymentInput = new
				{
					trxId = input.Id.ToString(),
					depositedAmount = input.Amount,
					depositedAccountId = bankInfo.AccountNumber,
					senderUser = new
					{
						firstname = client.FirstName,
						lastname = client.LastName,
						userIdentity = client.Id.ToString()
					},
					senderDescription = string.Empty
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = $"{baseUrl}{url}?apiToken={token}",
					PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
				};
				log.Info(JsonConvert.SerializeObject(httpRequestInput));
				var result = JsonConvert.DeserializeObject<Result>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
				if (result.Status == 1)
				input.ExternalTransactionId = result.ResultData;
				paymentSystemBl.ChangePaymentRequestDetails(input);
				return "ConfirmationCode";
			}
		}


		public static PaymentResponse PayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var baseUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.EliteUrl).StringValue;
				var token = GetToken(baseUrl, partnerPaymentSetting.Password);
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var bankCode = string.Empty;
				string accountName = paymentInfo.DocumentId;
				string iban = paymentInfo.AccountNumber;
				var paymentSystemName = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId).Name;
				if (paymentSystemName == Constants.PaymentSystems.EliteBankTransfer)
				{
					var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
					bankCode = bankInfo.BankCode;
					accountName = paymentInfo.BankAccountHolder;
					iban = paymentInfo.BankAccountNumber;
				}
				var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
				string url;
				switch (paymentSystem.Name)
				{
					case Constants.PaymentSystems.EliteBankTransfer:
						url = "/Api/Withdrawal/WithdrawalBankTransferCreate";
						break;
					case Constants.PaymentSystems.ElitePayFix:
						url = "/Api/Withdrawal/WithdrawalPayFixCreate";
						break;
					case Constants.PaymentSystems.ElitePapara:
						url = "/Api/Withdrawal/WithdrawalPaparaCreate";
						break;
					case Constants.PaymentSystems.EliteParazula:
						url = "/Api/Withdrawal/WithdrawalParazulaCreate";
						break;
					default:
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
				}
				var paymentInput = new 
				{
					receiverBankId = bankCode,
					receiverAccountName = accountName,
					receiverBankIban = iban,
                    withdrawalAmount = amount,
                    trxId = paymentRequest.Id.ToString()
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = $"{baseUrl}{url}?apiToken={token}",
					PostData = JsonConvert.SerializeObject(paymentInput)
				};
				var paymentOutput = JsonConvert.DeserializeObject<Result>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
				paymentRequest.ExternalTransactionId = paymentOutput.ResultData;
				paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
				return new PaymentResponse
				{
					Status = PaymentRequestStates.PayPanding,
				};
			}
		}

		public static string GetToken(string url, string apiKey)
		{
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Get,
				Url = $"{url}/Api/Security/GetToken?apiKey={apiKey}"
			};
			var token = JsonConvert.DeserializeObject<Result>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
			return token.ResultData;
		}
	}
}
