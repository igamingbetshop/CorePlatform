using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Elite;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class EliteController : ApiController
	{
		[HttpPost]
		[Route("api/Elite/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
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
				var userIds = new List<int>();
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							using (var documentBll = new DocumentBll(paymentSystemBl))
							{
								var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
								var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId)) ??
										 throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
								if (input.Status == "approved")
								{
									if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									{
										clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                    }
									else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
									{

										var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
										  null, null, false, string.Empty, documentBll, notificationBl, out userIds);
										clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
									}
									PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
									BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
									response = "OK";
								}
								else if (input.Status == "rejected")
								{
									paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
									if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
										clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.AdminDescription, notificationBl);
									else
										clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
											input.AdminDescription, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
								}
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
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