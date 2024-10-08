using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Collections.Generic;
using System.Web;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.PaymentGateway.Models.Yaspa;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Newtonsoft.Json.Linq;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class YaspaController : ApiController
	{
		[HttpPost]
		[Route("api/Yaspa/ApiRequest")]
		public HttpResponseMessage ApiRequest(PaymentInput input)
		{
			var response = "OK";
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
				var inputSign = HttpContext.Current.Request.Headers.Get("Citizen-Signature");
				WebApiApplication.DbLogger.Info("Citizen-Signature: " + inputSign);
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
					{
						var data = JsonConvert.DeserializeObject<Data>(JsonConvert.SerializeObject(input.data));
						var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(data.reference)) ??
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						if (paymentRequest.ClientId.Value.ToString() != data.customerIdentifier)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);
						var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																						   client.CurrencyId, paymentRequest.Type);
						var publicKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.YaspaPublicKey);
						var isVerified = VerifySignature(JsonConvert.SerializeObject(input.data), inputSign, publicKey);
						if (!isVerified)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
						if (paymentRequest.CurrencyId != data.paymentCurrency)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
						paymentRequest.ExternalTransactionId = data.citizenTransactionId;
						paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						using (var clientBl = new ClientBll(paymentSystemBl))
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						using (var documentBll = new DocumentBll(paymentSystemBl))
						{
							if (input.type == "PAYIN_COMPLETE" && data.transactionStatus == "COMPLETE")
							{
								if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
								{
									clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                }
								//else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
								//{
								//	var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
								//												null, null, false, paymentRequest.Parameters, documentBll, notificationBl);
								//	clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
								//}
							}
							else if (data.transactionStatus == "CANCELLED" || data.transactionStatus == "REJECTED_BY_ASPSP" ||
									 data.transactionStatus == "FAILED" || data.transactionStatus == "ERROR")
								if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, data.transactionStatus, notificationBl);
							//else
							//	clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Status,
							//											   null, null, false, string.Empty, documentBll, notificationBl);

							PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
							BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
						}
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail != null &&
					(ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
					ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
				{
					response = "OK";
				}
				else
				{
					response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error(response);
			}
			catch (Exception ex)
			{
				response = ex.Message;
				WebApiApplication.DbLogger.Error(response);
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}

			httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}

		public static bool VerifySignature(string webhookData, string webhookSignature, string citizenPublicKey)
		{
			var keyBytes = Convert.FromBase64String(citizenPublicKey);
			var keyParameter = PublicKeyFactory.CreateKey(keyBytes);
			var rsaKeyParameters = (RsaKeyParameters)keyParameter;
			var rsaParameters = new RSAParameters
			{
				Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned(),
				Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned()
			};
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportParameters(rsaParameters);

				using (SHA256 sha256 = SHA256.Create())
				{
					byte[] dataBytes = Encoding.UTF8.GetBytes(webhookData);
					byte[] hash = sha256.ComputeHash(dataBytes);

					bool verified = rsa.VerifyHash(hash, Convert.FromBase64String(webhookSignature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
					return verified;
				}
			}
		}

	}
}