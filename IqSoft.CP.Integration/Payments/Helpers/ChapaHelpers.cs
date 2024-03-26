using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Chapa;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public static class ChapaHelpers
	{
		public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			if (input.CurrencyId != Constants.Currencies.EthiopianBirr && input.CurrencyId != Constants.Currencies.USADollar)
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);

			var client = CacheManager.GetClientById(input.ClientId.Value);
			if (string.IsNullOrWhiteSpace(client.MobileNumber))
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
			var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ChapaUrl).StringValue;
			var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
			var data = new
			{
				key = partnerPaymentSetting.UserName,
				amount = input.Amount,
				currency = client.CurrencyId,
				tx_ref = input.Id.ToString(),
				callback_url = $"{paymentGateway}/api/Chapa/ApiRequest",
				return_url = cashierPageUrl,
				phone_number = client.MobileNumber.Replace("+", string.Empty)
			};

			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password.Split(',')[0] } },
				Url = $"{url}transaction/initialize",
				PostData = JsonConvert.SerializeObject(data)
			};

			var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
			if (response.Status == "success")
			{
				var dataOutput = JsonConvert.DeserializeObject<DataModel>(JsonConvert.SerializeObject(response.Data));
				return dataOutput.CheckoutUrl;
			}
			throw new Exception($"Error: {response.Status} {response.Message}");
		}

		public static void CheckTransactionStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var clientBl = new ClientBll(session, log))
			using (var paymentSystemBl = new PaymentSystemBll(clientBl))
			using (var documentBl = new DocumentBll(clientBl))
			using (var notificationBl = new NotificationBll(clientBl))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, paymentRequest.Type);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ChapaUrl).StringValue;
				var path = paymentRequest.Type == (int)PaymentRequestTypes.Withdraw ? "transfers" : "transaction";
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Get,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password.Split(',')[0] } },
					Url = $"{url}{path}/verify/{paymentRequest.Id}"
				};
				try
				{
					var response = JsonConvert.DeserializeObject<VerifyPaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
					if (response.Status == "success")
					{
						if (response.Data.Status == "success")
							if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
							{
								paymentRequest.ExternalTransactionId = response.Data.Reference;
								paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
							}								
							else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
							{
								paymentRequest.ExternalTransactionId = response.Data.ChapaTransferId;
								paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
								var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
																			   null, null, false, paymentRequest.Parameters, documentBl, notificationBl, false, true);
								clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
							}
							else if (response.Data.Status == "failed")
							{
								if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, response.Data.Status, notificationBl);
								else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
									clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, response.Data.Status,
																		null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
							}
					}
				}
				catch (FaultException<BllFnErrorType> ex)
				{
					log.Error(ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName);
				}
				catch (Exception ex)
				{
                    var output = JsonConvert.DeserializeObject<BaseOutput>(ex.Message);
                    var message = JsonConvert.DeserializeObject<BaseOutput>(output.Message);
                    if (message.Status == "failed")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, message.Message, notificationBl);
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, message.Message,
                                                                null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                    }
				}
			}
		}

		public static PaymentResponse PayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			if (paymentRequest.CurrencyId != Constants.Currencies.EthiopianBirr && paymentRequest.CurrencyId != Constants.Currencies.USADollar)
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ChapaUrl).StringValue;
				var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
				var paymentInput = new
				{
					account_name = paymentInfo.BankAccountHolder,
					account_number = paymentInfo.BankAccountNumber,
					currency = client.CurrencyId,
					amount,
					reference = paymentRequest.Id.ToString(),
					bank_code = bankInfo.BankCode
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password.Split(',')[0] } },
					Url = $"{url}transfers",
					PostData = JsonConvert.SerializeObject(paymentInput)
				};
				try
				{
					var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
					log.Info("Chapa_withdraw_Response: " + response);
					var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
					if (output.Status == "success" || (output.Status == "failed" && output.Message == "The reference number should be unique."))
						return new PaymentResponse
						{
							Status = PaymentRequestStates.PayPanding,
						};
					throw new Exception($"Error: {output.Status} {output.Message}");
				}
				catch (Exception ex)
				{
					var output = JsonConvert.DeserializeObject<BaseOutput>(ex.Message);
					var message = JsonConvert.DeserializeObject<BaseOutput>(output.Message);
					if (message.Status == "failed")
					{
						if(message.Message == "The reference number should be unique.")
						{
							return new PaymentResponse
							{
								Status = PaymentRequestStates.PayPanding,
							};
						}
						else if (message.Message == "Invalid account number for the selected bank or mobile wallet type.")
						{
							using (var clientBl = new ClientBll(paymentSystemBl))
							using (var documentBl = new DocumentBll(paymentSystemBl))
							using (var notificationBl = new NotificationBll(paymentSystemBl))
								clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, message.Message,
																	null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
						}
						throw new Exception($"Error: {message.Status} {message.Message}");
					}
					throw;
				}
			}
		}
	}
}