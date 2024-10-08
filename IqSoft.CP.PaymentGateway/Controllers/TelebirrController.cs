using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Telebirr;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class TelebirrController : ApiController
	{
		[HttpPost]
		[Route("api/Telebirr/ApiRequest/{partnerId}")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage,int partnerId)
		{
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
				WebApiApplication.DbLogger.Info(inputString);
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
					using (var partnerBl = new PartnerBll(clientBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
								using (var documentBll = new DocumentBll(paymentSystemBl))
								{
									var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Telebirr);
									var publicKey = partnerBl.GetPaymentValueByKey(partnerId, paymentSystem.Id, Constants.PartnerKeys.TelebirrPublicKey);
									
									string data = DecryptByPublicKey(inputString, publicKey);
									var input = JsonConvert.DeserializeObject<PaymentInput>(data);
									if (input.TradeStatus == 2)
									{
										var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OutTradeNo)) ??
												 throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
										paymentRequest.ExternalTransactionId = input.TransactionNo;
										clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                        foreach (var uId in userIds)
                                        {
                                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                        }
                                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
										BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
										response = JsonConvert.SerializeObject(new
										{
											code = 0,
											msg = "success"
										});
									}
								}
							}
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
					response = JsonConvert.SerializeObject(new
					{
						code = 0,
						msg = "success"
					});
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

		public static string DecryptByPublicKey(string input, string publicKey)
		{
			var MaxDecryptBlock = 256;
			RsaKeyParameters publicKeyParams = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
			var data = Convert.FromBase64String(input);
			var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
			cipher.Init(false, publicKeyParams);
			int inputLen = data.Length;
			using (MemoryStream outStream = new MemoryStream())
			{
				int offset = 0;
				byte[] cache;
				int i = 0;

				while (inputLen - offset > 0)
				{
					if (inputLen - offset > MaxDecryptBlock)
					{
						cache = cipher.DoFinal(data, offset, MaxDecryptBlock);
					}
					else
					{
						cache = cipher.DoFinal(data, offset, inputLen - offset);
					}
					outStream.Write(cache, 0, cache.Length);
					i++;
					offset = i * MaxDecryptBlock;
				}
				return Encoding.UTF8.GetString(outStream.ToArray());
			}
		}
	}
}
