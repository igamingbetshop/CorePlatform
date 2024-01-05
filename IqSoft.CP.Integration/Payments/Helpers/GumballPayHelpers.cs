using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.GumballPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;
using System.Web;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class GumballPayHelpers
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
						if (string.IsNullOrEmpty(client.FirstName))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
						if (string.IsNullOrEmpty(client.LastName))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
						if (string.IsNullOrWhiteSpace(client.MobileNumber))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
						if (string.IsNullOrWhiteSpace(client.ZipCode))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
						if (string.IsNullOrWhiteSpace(client.Email))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
						if (string.IsNullOrWhiteSpace(client.Address))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
						var partner = CacheManager.GetPartnerById(client.PartnerId);
						var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
						var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GumballPayUrl).StringValue;
						var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
						var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
						var amount = ((double)input.Amount * 100);
						var hash = $"{partnerPaymentSetting.UserName}{input.Id}{amount}{client.Email}{partnerPaymentSetting.Password}";
						var control = CommonFunctions.ComputeSha1(hash);
						var data = new
						{
							client_orderid = input.Id.ToString(),
							order_desc = partner.Name,
							amount = input.Amount,
							currency = client.CurrencyId,
							address1 = client.Address,
							city = paymentInfo.City,
							zip_code = client.ZipCode,
							country = paymentInfo.Country,
							phone = client.MobileNumber.Replace("+", string.Empty),
							email = client.Email,
							ipaddress = session.LoginIp,
							control,
							first_name = client.FirstName,
							last_name = client.LastName,
							redirect_url = cashierPageUrl,
							server_callback_url = string.Format("{0}/api/GumballPay/ApiRequest", paymentGateway),
						};
						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
							RequestMethod = Constants.HttpRequestMethods.Post,
							Url = $"{url}sale-form/group/{partnerPaymentSetting.UserName}",
							PostData = CommonFunctions.GetUriEndocingFromObject(data)
						};
						log.Info(JsonConvert.SerializeObject(httpRequestInput));

						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var dict = HttpUtility.ParseQueryString(response);
						var json = new JavaScriptSerializer().Serialize(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
						var output = JsonConvert.DeserializeObject<PaymentOutput>(json);
						if (output.Type.Trim() == "async-form-response")
						{
							return output.RedirectUrl;
						}
						else
						{
							throw new Exception($"Error: {output.ErrorCode} {output.ErrorMessage}");
						}
					}
				}
			}
		}
	}

	//public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
	//{
	//	using (var paymentSystemBl = new PaymentSystemBll(session, log))
	//	{
	//		using (var documentBl = new DocumentBll(paymentSystemBl))
	//		{
	//			using (var clientBl = new ClientBll(paymentSystemBl))
	//			{
	//				var client = CacheManager.GetClientById(input.ClientId.Value);
	//				if (string.IsNullOrEmpty(client.FirstName))
	//					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
	//				if (string.IsNullOrEmpty(client.LastName))
	//					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
	//				if (string.IsNullOrWhiteSpace(client.MobileNumber))
	//					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
	//				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
	//				var accountId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaAccountId).StringValue;
	//				var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}");
	//				var amount = input.Amount - (input.CommissionAmount ?? 0);
	//				var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
	//				var paymentMethod = CacheManager.GetPartnerSettingByKey(client.PartnerId, paymentSystem.Name).StringValue.Split(',');
	//				var headers = new Dictionary<string, string>
	//				{
	//					{ "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
	//					{ "Ik-Api-Account-Id", accountId}
	//				};
	//				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
	//				string bankName = null;
	//				string iban = null;
	//				string card = paymentInfo.CardNumber;
	//				var accountNumber = paymentInfo.WalletNumber;
	//				var documentId = paymentInfo.DocumentId;
	//				if (paymentSystem.Name == Constants.PaymentSystems.InterkassaPapara)
	//					accountNumber = paymentInfo.AccountNumber;
	//				if (paymentSystem.Name == Constants.PaymentSystems.InterkassaTrBanking)
	//				{
	//					bankName = paymentInfo.BankName;
	//					documentId = paymentInfo.CardNumber;
	//					accountNumber = paymentInfo.BankAccountNumber;
	//					iban = paymentInfo.BankACH;
	//					card = null;
	//				}
	//				var payoutInput = new PayoutInput()
	//				{
	//					amount = amount,
	//					method = paymentMethod[0],
	//					currency = paymentMethod[1],
	//					useShortAlias = true,
	//					purseId = paymentMethod[2],
	//					action = "process",
	//					calcKey = "ikPayerPrice",
	//					paymentNo = input.Id
	//				};
	//				var paymentDetails = new Dictionary<string, string>()
	//				{
	//					{ "first_name", client.FirstName },
	//					{ "last_name", client.LastName},
	//					{ "email", client.Email},
	//					{ "account_number", accountNumber },
	//					{ "phone", client.MobileNumber.Replace("+", string.Empty)},
	//					{ "document_id", documentId },
	//					{ "iban", iban },
	//					{ "bank_identity", bankName },
	//					{ "card", card }
	//				};
	//				var details = string.Empty;
	//				foreach (var detail in paymentDetails)
	//				{
	//					if (!string.IsNullOrEmpty(detail.Value))
	//					{
	//						var item = $"&details[{detail.Key}]={detail.Value}";
	//						details = details + item;
	//					}
	//				}
	//				var postData = CommonFunctions.GetUriDataFromObject<PayoutInput>(payoutInput) + details;
	//				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaUrl).StringValue;
	//				var httpRequestInput = new HttpRequestInput
	//				{
	//					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
	//					RequestMethod = Constants.HttpRequestMethods.Post,
	//					RequestHeaders = headers,
	//					Url = string.Format(url, "api") + "/v1/withdraw",
	//					PostData = postData
	//				};
	//				log.Info(string.Format("PostData: {0}", postData));
	//				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
	//				log.Info(string.Format("Message: {0}", response));
	//				var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
	//				if (output.Status == "ok")
	//				{
	//					var data = JsonConvert.DeserializeObject<Data>(JsonConvert.SerializeObject(output.Data));
	//					input.ExternalTransactionId = data?.Transaction?.OpId;
	//					paymentSystemBl.ChangePaymentRequestDetails(input);
	//					return new PaymentResponse
	//					{
	//						Status = PaymentRequestStates.PayPanding,
	//					};
	//				}
	//				else
	//					throw new Exception(string.Format("ErrorMessage: {0}", output.Message));
	//			}
	//		}
	//	}
	//}

}


