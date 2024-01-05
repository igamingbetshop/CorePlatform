using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.SantimaPay;
using log4net;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class SantimaPayHelpers
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
						if (string.IsNullOrWhiteSpace(client.MobileNumber))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
						var partner = CacheManager.GetPartnerById(client.PartnerId);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
						var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SantimaPayUrl).StringValue;
						var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
						var signature = GenerateSignedToken(new Dictionary<string, object>()
						{
							{ "amount", input.Amount.ToString() },
							{ "paymentreason", "Deposit"},
							{ "merchantId", partnerPaymentSetting.UserName }
						}, partnerPaymentSetting.Password);

						var data = new 
						{
							Id = input.Id.ToString(),
							Reason = "Deposit",
							Amount = input.Amount.ToString(),
							MerchantId = partnerPaymentSetting.UserName,
							SuccessRedirectUrl = cashierPageUrl,
							FailureRedirectUrl = cashierPageUrl,
							CancelRedirectUrl = cashierPageUrl,
							NotifyUrl = string.Format("{0}/api/SantimaPay/ApiRequest", paymentGateway),
							PhoneNumber = client.MobileNumber,
							SignedToken = signature
						};

						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationJson,
							RequestMethod = Constants.HttpRequestMethods.Post,
							Url = $"{url}initiate-payment",
							PostData = JsonConvert.SerializeObject(data)
						};

						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
						if (!string.IsNullOrEmpty(output.Url))
							return output.Url;
						else
							throw new Exception($"Error: {output.Reason}");
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
				var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SantimaPayUrl).StringValue;
				var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
				var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
					throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
				var signature = GenerateSignedToken(new Dictionary<string, object>()
						{
							{ "amount", paymentRequest.Amount},
							{ "paymentreason", "Withdraw"},
							{ "merchantId", partnerPaymentSetting.UserName }
						}, partnerPaymentSetting.Password);
				var paymentInput = new
				{
					amount = paymentRequest.Amount,
					clientReference = paymentRequest.Id.ToString(),
					id = bankInfo.BankCode,
					merchantId = partnerPaymentSetting.UserName,
					paymentMethod = bankInfo.NickName,
					reason = "Withdraw",
					receiverAccountNumber = paymentInfo.BankAccountNumber,
					signedToken = signature,
				};
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = $"{url}payout-transfer",
					PostData = JsonConvert.SerializeObject(paymentInput)
				};
				var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
				if (output.Status == "SUCCESS")
				{
					return new PaymentResponse
					{
						Status = PaymentRequestStates.Approved,
					};
				}
				else
				   throw new Exception($"Error: {output.Status} {output.Message}");
			}
		}

		private static ECDsa GetEllipticCurveAlgorithm(string privateKey)
		{
			var keyParams = (ECPrivateKeyParameters)PrivateKeyFactory
				.CreateKey(Convert.FromBase64String(privateKey));

			var normalizedECPoint = keyParams.Parameters.G.Multiply(keyParams.D).Normalize();
			var asss = ECDsa.Create(new ECParameters
			{
				Curve = ECCurve.CreateFromValue(keyParams.PublicKeyParamSet.Id),
				D = keyParams.D.ToByteArrayUnsigned(),
				Q =
				{
					X = normalizedECPoint.XCoord.GetEncoded(),
					Y = normalizedECPoint.YCoord.GetEncoded()
				}
			});
			return ECDsa.Create(new ECParameters
			{
				Curve = ECCurve.CreateFromValue(keyParams.PublicKeyParamSet.Id),
				D = keyParams.D.ToByteArrayUnsigned(),
				Q =
		        {
		        	X = normalizedECPoint.XCoord.GetEncoded(),
		        	Y = normalizedECPoint.YCoord.GetEncoded()
		        }
			});
		}

		public static string GenerateSignedToken(Dictionary<string, object> claims, string privateKey)
		{
			long now = DateTimeOffset.Now.ToUnixTimeSeconds();
			var handler = new JwtSecurityTokenHandler();
			handler.SetDefaultTimesOnTokenCreation = false;

			var signatureAlgorithm = GetEllipticCurveAlgorithm(privateKey);
			var eCDsaSecurityKey = new ECDsaSecurityKey(signatureAlgorithm)
			{
				CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
			};

			claims["generated"] = now;

			var token = handler.CreateToken(new SecurityTokenDescriptor
			{
				Claims = claims,
				SigningCredentials = new SigningCredentials(eCDsaSecurityKey, SecurityAlgorithms.EcdsaSha256)
			});

			var tokenString = handler.WriteToken(token);

			return tokenString;
		}
	}
}