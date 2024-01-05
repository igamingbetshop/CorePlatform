using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.CardPay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[ApiController]
	public class CardPayController : ControllerBase
	{
		private static readonly List<string> WhitelistedIps = new List<string>
		{
			"54.220.175.23",
			"3.124.131.62",
			"104.22.39.79",
			"104.22.38.79",
			"172.67.12.28",
			"18.158.236.50",
			"54.216.23.40"
		};

		[HttpPost]
		[Route("api/CardPay/ApiRequest")]
		public ActionResult ApiRequest(JObject inp)
		{
			var response = string.Empty;
			try
			{
				using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
				using var clientBl = new ClientBll(paymentSystemBl);
				using var documentBl = new DocumentBll(paymentSystemBl);
				using var notificationBl = new NotificationBll(clientBl);

				var bodyStream = new StreamReader(Request.Body);
				var inputString = bodyStream.ReadToEnd();
				Program.DbLogger.Info(inputString);
				var ip = string.Empty;
				if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();
				BaseBll.CheckIp(WhitelistedIps, ip);
				var input = JsonConvert.DeserializeObject<RequestResultInput>(inputString);
				var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Order.Id));
				if (request == null)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

				var client = CacheManager.GetClientById(request.ClientId);
				if (client == null)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
					request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
				var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CardPaySecretKey).StringValue;
				var auth = Request.Headers["Signature"];
				Program.DbLogger.Info("auth: " + auth);
				var signString = CommonFunctions.ComputeSha512(inputString + secretKey);
				if (signString.ToLower() != auth.ToString().ToLower())
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

				var isDeposit = request.Type == (int)PaymentRequestTypes.Deposit;
				var status = isDeposit ? input.PaymentDetails.Status.ToUpper() : input.PayoutDetails.Status.ToUpper();
				if (status == "COMPLETED")
				{
					if (isDeposit)
					{
						request.Info = JsonConvert.SerializeObject(input.CardAccount);
						request.ExternalTransactionId = input.PaymentDetails.Id.ToString();
						paymentSystemBl.ChangePaymentRequestDetails(request);
						clientBl.ApproveDepositFromPaymentSystem(request, false);
					}
					else
					{
						request.ExternalTransactionId = input.PayoutDetails.Id.ToString();
						var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
							request.CashDeskId, null, false, request.Parameters, documentBl, notificationBl);
						clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
						PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
					}
					response = "OK";

					return Ok(new StringContent(response, Encoding.UTF8));
				}
				else if (status == "DECLINED")
				{
					if (isDeposit)
						clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);
					else
						clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.PayoutDetails.DeclineReason,
															null, null, false, string.Empty, documentBl, notificationBl);
				}
				else
				{
					response = "Error";

					Response.StatusCode = (int)HttpStatusCode.Conflict;
					return Ok(new StringContent(response, Encoding.UTF8));
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail != null &&
					(ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
					ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
				{
					response = "OK";
					return Ok(new StringContent(response, Encoding.UTF8));

				}
				var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

				Program.DbLogger.Error(exp);
				Response.StatusCode = (int)HttpStatusCode.Conflict;
				return Ok(new StringContent(exp.Message, Encoding.UTF8));

			}
			catch (Exception ex)
			{
				Program.DbLogger.Error(ex);
			}

			Response.StatusCode = (int)HttpStatusCode.Conflict;
			return Ok(new StringContent("", Encoding.UTF8));
		}
	}
}