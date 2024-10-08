using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Paylado;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class PayladoHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(input.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																				   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayladoUrl).StringValue;
				var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
				var successPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentSuccessPageUrl).StringValue;
				var paymentInput = new
				{
					MerchantId = partnerPaymentSetting.UserName,
					MerchantGuid = partnerPaymentSetting.Password,
					ReturnUrl = cashierPageUrl,
					TransactionUrl = url,
					TransactionType = "Sale",
					Amount = input.Amount,
					Currency = client.CurrencyId,
					FirstName = client.FirstName,
					Lastname = client.LastName,
					Email = client.Email,
					NotificationUrl = string.Format("{0}/api/Paylado/ApiRequest", paymentGateway),
					TransactionReference = input.Id.ToString(),
					ext_client_id = client.Id
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = string.Format("{0}/tokenizer/get/", url),
					PostData = CommonFunctions.GetUriEndocingFromObject(paymentInput)
				};
				log.Info($"PostData: {httpRequestInput.PostData}");
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(res);
				input.ExternalTransactionId = paymentOutput.Token;
				paymentSystemBl.ChangePaymentRequestDetails(input);

				if (paymentOutput.ResultStatus != "OK")
					throw new Exception(string.Format("ResultStatus: {0}, ResultCode: {1}, ResultMessage: {2}", paymentOutput.ResultStatus,
										 paymentOutput.ResultCode, paymentOutput.ResultMessage));

				return paymentOutput.RedirectUrl;
			}
		}


		public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var clientBl = new ClientBll(paymentSystemBl))
				{

					var client = CacheManager.GetClientById(input.ClientId.Value);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																					   input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
					var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayladoUrl).StringValue;
					var amount = input.Amount - (input.CommissionAmount ?? 0);

					var clientWalletInfo = clientBl.GetClientPaymentAccountDetails(client.Id, input.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
											  .FirstOrDefault();
					if (clientWalletInfo == null)
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
					var payoutInput = new
					{
						MerchantId = partnerPaymentSetting.UserName,
						MerchantGuid = partnerPaymentSetting.Password,
						Amount = amount.ToString(CultureInfo.InvariantCulture), 
						Currency = client.CurrencyId,
						PaymentOptionAlias = clientWalletInfo.WalletNumber,
						IpAddress = session.LoginIp,
						TransactionReference = input.Id.ToString()
					};
					var httpRequestInput = new HttpRequestInput
					{
						ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
						RequestMethod = Constants.HttpRequestMethods.Post,
						Url = string.Format("{0}/process/payout", url),
						PostData = CommonFunctions.GetUriEndocingFromObject(payoutInput)
					};
					log.Info($"PostData: {httpRequestInput.PostData}");
					var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
					log.Info($"Response: {res}");
					var output = JsonConvert.DeserializeObject<PayoutOutput>(res);
					input.ExternalTransactionId = output.TransactionId;
					paymentSystemBl.ChangePaymentRequestDetails(input);

					return new PaymentResponse
					{
						Status = PaymentRequestStates.PayPanding,
					};
				}
			}
		}

		public static List<int> GetTransactionDetails(PaymentRequest input, SessionIdentity session, ILog log)
		{
			var userIds = new List<int>();
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(input.ClientId.Value);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayladoUrl).StringValue;
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
																				   input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var payoutInput = new
				{
					MerchantId = partnerPaymentSetting.UserName,
					MerchantGuid = partnerPaymentSetting.Password,
					TransactionReference = input.Id.ToString(),
					Format = "json"
				};

				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = string.Format("{0}/tokenizer/getresult/", url),
					PostData = CommonFunctions.GetUriEndocingFromObject(payoutInput)
				};
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<ResultOutput>(res);

				using (var clientBl = new ClientBll(session, log))
				{
					using (var documentBl = new DocumentBll(clientBl))
					{
						using (var notificationBl = new NotificationBll(clientBl))
						{
							if (output.ResultStatus == "OK")
							{
								if (input.Type == (int)PaymentRequestTypes.Deposit)
								{
									var info = clientBl.GetClientPaymentAccountDetails(client.Id, input.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
											.FirstOrDefault();
									if (info == null)
									{
										info = clientBl.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
										{
											AccountNickName = CacheManager.GetPaymentSystemById(input.PaymentSystemId).Name,
											Type = (int)ClientPaymentInfoTypes.Wallet,
											ClientId = input.ClientId.Value,
											PartnerPaymentSystemId = partnerPaymentSetting.Id,
											WalletNumber = output.PaymentOptionAlias,
											CreationTime = input.CreationTime,
											LastUpdateTime = input.LastUpdateTime,
										}, null, false);
									}

									clientBl.ApproveDepositFromPaymentSystem(input, false, out userIds, info: info);
								}
								else if (input.Type == (int)PaymentRequestTypes.Withdraw)
								{
									var resp = clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Approved,
										string.Empty, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
									clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
								}
							}
							else if (output.ResultStatus == "DECLINE" || output.ResultStatus == "ERROR" || output.ResultStatus == "NOTRANS")
							{
								var reason = string.Format("ResultStatus: {0}, ResultMessage: {1}", output.ResultStatus, output.ResultMessage);
								if (input.Type == (int)PaymentRequestTypes.Deposit)
								{
									clientBl.ChangeDepositRequestState(input.Id, PaymentRequestStates.Failed, reason, notificationBl);
								}
								else if (input.Type == (int)PaymentRequestTypes.Withdraw)
								{
									clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Failed,
										reason, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
								}
							}
						}
					}
				}
			}
			return userIds;
        }
	}
}

