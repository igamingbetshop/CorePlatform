using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.GumballPay;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class GumballPayController : ApiController
	{
		[Route("api/GumballPay/ApiRequest")]
		public HttpResponseMessage ApiRequest([FromBody] PaymentInput input)
		{
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Merchant_Order));
							if (paymentRequest == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
							var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
							if (partnerPaymentSetting == null)
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
							var hash = $"{input.Status}{input.Orderid}{input.Merchant_Order}{partnerPaymentSetting.Password}";
							if (CommonFunctions.ComputeSha1(hash) != input.Control)
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
							paymentRequest.ExternalTransactionId = input.Orderid;
							paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
							if (input.Status == "approved")
							{
								WebApiApplication.DbLogger.Info("Log before approving");
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
								PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
								BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
							}
							else
							{
								var message = $"ErrorCode: {input.Error_Code} ErrorMessage: {input.Error_Message}";
								clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, message, notificationBl);
							}
							response = "OK";
						}
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
										  ex.Detail.Id == Constants.Errors.RequestAlreadyPayed || ex.Detail.Id == Constants.Errors.CanNotCancelPayedRequest))
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
				response = $"Message: {ex.Message} InnerException: {ex.InnerException.InnerException.Message}";
				WebApiApplication.DbLogger.Error(response);
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
			return httpResponseMessage;
		}
	}
}