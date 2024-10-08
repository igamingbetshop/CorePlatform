using IqSoft.CP.Common;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.JetonHavale;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using System;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;
using IqSoft.CP.PaymentGateway.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class JetonHavaleController : ApiController
	{

		[HttpPost]
		[Route("api/JetonHavale/ApiRequest")]
		public HttpResponseMessage ApiRequest(PaymentInput input)
		{
			var response = string.Empty;
			var userIds = new List<int>();
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			}; try
			{
				WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
							if (paymentRequest == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
							var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
														paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
							var hashString = $"{partnerPaymentSetting.Password}transactionId={input.TransactionId}&amount={input.Amount}&status={input.Status}";
							if (input.Hash.ToLower() != CommonFunctions.ComputeSha1(hashString).ToLower())
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
							if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
							{
								if (input.Status == "CONFIRMED")
								{
									if(input.Amount != paymentRequest.Amount)
										paymentRequest.Amount = input.Amount;
									clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                }
								else if (input.Status == "REJECTED" || input.Status == "FAILED")
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, null, notificationBl);
							}
							else if (paymentRequest.Type == (int) PaymentRequestTypes.Withdraw)
							{

								using (var documentBll = new DocumentBll(paymentSystemBl))
								{
									if (input.Status == "CONFIRMED")
									{
										var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
										 null, null, false, string.Empty, documentBll, notificationBl, out userIds);
										clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
									}
									else if (input.Status == "REJECTED" || input.Status == "FAILED")
									{
										clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
											string.Empty, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
									}
								}
							}
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            response = "OK";
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
	}
}