using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Katarun;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class KatarunController : ApiController
	{
		[HttpPost]
		[Route("api/Katarun/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var inputSign = HttpContext.Current.Request.Headers.Get("X-Signature");
			WebApiApplication.DbLogger.Info("X-Signature " + inputSign);
			var response = "OK";
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
					{
						var input = JsonConvert.DeserializeObject<PaymentOutput>(inputString);
						var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.reference)) ??
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
						var secretKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.KatarunSecretKey);
						var isVerified = VerifySignature(inputString, inputSign, secretKey);
						if (!isVerified)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
						paymentRequest.ExternalTransactionId = input.id;
						paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						using (var clientBl = new ClientBll(paymentSystemBl))
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						using (var documentBll = new DocumentBll(paymentSystemBl))
						{
							if (input.status == "paid")
							{
								if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
								else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
								{
									var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
																				null, null, false, paymentRequest.Parameters, documentBll, notificationBl);
									clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
								}
							}
							else if (input.status == "error" || input.status == "cancelled" || input.status == "blocked")
							{

								var errorMessage = new Error();
								var error = input.transaction_data?.attempts?.FirstOrDefault()?.error;
								if (error != null)
									errorMessage = JsonConvert.DeserializeObject<Error>(JsonConvert.SerializeObject(error));
								if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, $"Status: {input.status} Code: {errorMessage.code} Message: {errorMessage.message}", notificationBl);
								else
									clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, $"Status: {input.status} Code: {errorMessage.code} Message: {errorMessage.message}",
																			   null, null, false, string.Empty, documentBll, notificationBl);
							}
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

		public static bool VerifySignature(string webhookData, string signature, string secretKeyy)
		{
			var keyBytes = Convert.FromBase64String(secretKeyy);
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

					bool verified = rsa.VerifyHash(hash, Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
					return verified;
				}
			}
		}
	}
}