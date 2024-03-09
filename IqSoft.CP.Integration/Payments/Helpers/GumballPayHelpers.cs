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
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models;

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
						//if (string.IsNullOrWhiteSpace(client.ZipCode))
						//	throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
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
						var errorPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentErrorPageUrl).StringValue;
						var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
						if (distributionUrlKey == null || distributionUrlKey.Id == 0)
							distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
						var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
						var data = new
						{
							client_orderid = input.Id.ToString(),
							order_desc = partner.Name,
							amount = input.Amount,
							currency = client.CurrencyId,
							address1 = client.Address,
							city = paymentInfo.City,
							zip_code = "12345", // client.ZipCode,
							country = paymentInfo.Country,
							phone = client.MobileNumber.Replace("+", string.Empty),
							email = client.Email,
							ipaddress = session.LoginIp,
							control,
							first_name = client.FirstName,
							last_name = client.LastName,
							//redirect_url = cashierPageUrl + "?status=${status}" ,
							redirect_success_url = string.Format("{0}/redirect/RedirectRequest?redirectUrl={1}", distributionUrl, cashierPageUrl), 
							redirect_fail_url = string.Format("{0}/redirect/RedirectRequest?redirectUrl={1}", distributionUrl, errorPageUrl),
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


		public static PaymentResponse ReturnRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																				   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GumballPayUrl).StringValue;
				var depositExternalId = "";
				var login = partnerPaymentSetting.UserName.Split(',');
				var hash = $"{login[1]}{paymentRequest.Id}{depositExternalId}{paymentRequest.Amount * 100}{client.CurrencyId}{partnerPaymentSetting.Password}";
				var control = CommonFunctions.ComputeSha1(hash);
				var data = new
				{
					login = login[1],
					client_orderid = paymentRequest.Id,
					orderid = depositExternalId,
					amount = paymentRequest.Amount,
					currency = client.CurrencyId,
					comment = "Return",
					control
				};

				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = $"{url}return/group/{login[0]}",
					PostData = CommonFunctions.GetUriEndocingFromObject(data)
				};
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var dict = HttpUtility.ParseQueryString(response);
				var json = new JavaScriptSerializer().Serialize(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
				var output = JsonConvert.DeserializeObject<PaymentOutput>(json);
				if (output.Type.Trim() == "async-response")
				{
					return new PaymentResponse
					{
						Status = PaymentRequestStates.Approved,
					};
				}
				else
					throw new Exception($"Error: {output.ErrorCode} {output.ErrorMessage}");
			}
		}
	}
}


