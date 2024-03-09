using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Telebirr;
using log4net;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
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
						var partner = CacheManager.GetPartnerById(client.PartnerId);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
						var keys = partnerPaymentSetting.UserName.Split(',');
						var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Telebirr);
						var publicKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.TelebirrPublicKey);
						var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TelebirrUrl).StringValue;
						var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
						var datatime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
						var data = new PaymentInput
						{
							appId = keys[0],
							nonce = input.Id.ToString(),
							notifyUrl = string.Format($"{paymentGateway}/api/Telebirr/ApiRequest/{client.PartnerId}"),
							outTradeNo = input.Id.ToString(),
							returnUrl = cashierPageUrl,
							shortCode = keys[1],
							subject = "Deposit",
							timeoutExpress = "30",
							timestamp = datatime,
							totalAmount = input.Amount.ToString(),
							receiveName = partner.Name
						};
						var a = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
						string ussd = EncryptByPublicKey(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), publicKey);						
						data.appKey = partnerPaymentSetting.Password;
						var orderdParams = CommonFunctions.GetSortedParamWithValuesAsString(data, "&", false);
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

						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
						if (output.Message == "Operation successful")
							return output.UrlData.ToPayUrl;
						else
							throw new Exception($"Error: {output.Message}");
					}
				}
			}
		}

		public static string EncryptByPublicKey(string input, string publicKey)
		{
			var maxEncryptBlock = 117;
			try
			{
				RsaKeyParameters publicKeyParams = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
				var data = Encoding.UTF8.GetBytes(input);
				var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
				cipher.Init(true, publicKeyParams);

				using (var memoryStream = new MemoryStream())
				{
					int inputLen = data.Length;
					int offSet = 0;
					byte[] cache;
					int i = 0;

					while (inputLen - offSet > 0)
					{
						if (inputLen - offSet > maxEncryptBlock)
						{
							cache = cipher.DoFinal(data, offSet, maxEncryptBlock);
						}
						else
						{
							cache = cipher.DoFinal(data, offSet, inputLen - offSet);
						}
						memoryStream.Write(cache, 0, cache.Length);
						i++;
						offSet = i * maxEncryptBlock;
					}

					return Convert.ToBase64String(memoryStream.ToArray());
				}
			}
			catch (Exception e)
			{
				throw new ApplicationException(e.Message, e);
			}
		}		
	}
}

