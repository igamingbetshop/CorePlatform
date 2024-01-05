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
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class TelebirrHelpers
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
						var keys = partnerPaymentSetting.UserName.Split(',');
						var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Telebirr);
						var publicKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.TelebirrPublicKey);
						var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TelebirrUrl).StringValue;
						var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
						var data = new
						{
							appId = keys[0],
							appKey = partnerPaymentSetting.Password,
							nonce = input.Id.ToString(),
							notifyUrl = string.Format("{0}/api/Telebirr/ApiRequest", paymentGateway),
							returnUrl = cashierPageUrl,
							outTradeNo = input.Id.ToString(),
							shortCode = keys[1],
							subject = "Deposit",
							timeoutExpress = "30",
							timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
							totalAmount = input.Amount.ToString()
						};
						var ussd = GenerateSignedToken(JsonConvert.SerializeObject(data), publicKey);
						var orderdParams = CommonFunctions.GetSortedParamWithValuesAsString(data, "&");
						var sign = CommonFunctions.ComputeSha256(orderdParams);
						var postData = new
						{
							appid = keys[0],
							sign,
							ussd
						};
						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationJson,
							RequestMethod = Constants.HttpRequestMethods.Post,
							Url = url,
							PostData = JsonConvert.SerializeObject(postData)
						};
						var ddddd = JsonConvert.SerializeObject(data);

						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						//var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
						//if (!string.IsNullOrEmpty(output.Url))
						//	return output.Url;
						//else
						//	throw new Exception($"Error: {output.Reason}");
						return null;
					}
				}
			}
		}

		public static string GenerateSignedToken(string jsonData, string publicKey)
		{
			byte[] jsonDataBytes = Encoding.UTF8.GetBytes(jsonData);
			byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
			using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
			{
				RSAParameters rsaParameters = new RSAParameters
				{
					Modulus = publicKeyBytes,
					Exponent = new byte[] { 1, 0, 1 } 
				};
				rsa.ImportParameters(rsaParameters);
				byte[] encryptedData = rsa.Encrypt(jsonDataBytes, false);
				string encryptedBase64 = Convert.ToBase64String(encryptedData);
				return encryptedBase64;
			}

		}
	}
}
