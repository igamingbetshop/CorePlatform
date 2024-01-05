using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Chapa;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class ChapaHelpers
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
							return_url = cashierPageUrl
						};

						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationJson,
							RequestMethod = Constants.HttpRequestMethods.Post,
							RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
							Url = $"{url}transaction/initialize",
							PostData = JsonConvert.SerializeObject(data)
						};

						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
						if (output.Status == "success")
						{
							var dataOutput = JsonConvert.DeserializeObject<Data>(JsonConvert.SerializeObject(output.Data));
							return dataOutput.CheckoutUrl;
						}
						else
						{
							throw new Exception($"Error: {output.Status} {output.Message}");
						}
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
					amount = amount,
					reference = paymentRequest.Id.ToString(),
					bank_code = bankInfo.BankCode
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
					Url = $"{url}transfers",
					PostData = JsonConvert.SerializeObject(paymentInput)
				};
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
				if (output.Status == "success")
				{
					return new PaymentResponse
					{
						Status = PaymentRequestStates.PayPanding,
					};
				}
				else
				{
					throw new Exception($"Error: {output.Status} {output.Message}");
				}
				
			}
		}
	}
}
