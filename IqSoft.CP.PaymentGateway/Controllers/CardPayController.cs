using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.CardPay;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class CardPayController : ApiController
    {
		public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.CardPay);

		[HttpPost]
        [Route("api/CardPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(JObject inp)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
				using (var clientBl = new ClientBll(paymentSystemBl))
				{
					using (var documentBl = new DocumentBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(clientBl))
						{
							try
							{
								var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
								var inputString = bodyStream.ReadToEnd();
								WebApiApplication.DbLogger.Info(inputString);
								BaseBll.CheckIp(WhitelistedIps);
								var input = JsonConvert.DeserializeObject<RequestResultInput>(inputString);
								var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Order.Id));
								if (request == null)
									throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

								var client = CacheManager.GetClientById(request.ClientId.Value);
								if (client == null)
									throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

								var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
									request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
								var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CardPaySecretKey).StringValue;
								var auth = HttpContext.Current.Request.Headers.Get("Signature");
								WebApiApplication.DbLogger.Info("auth: " + auth);
								var signString = CommonFunctions.ComputeSha512(inputString + secretKey);
								if (signString.ToLower() != auth.ToLower())
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
										PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
									}
									response = "OK";
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
									return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
								}
								else if (status == "DECLINED")
								{
									if (isDeposit)
										clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);
									else
										clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, 
											input.PayoutDetails.DeclineReason, null, null, false, string.Empty, documentBl, notificationBl);
								}
								else
								{
									response = "Error";
									return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
								}
							}
							catch (FaultException<BllFnErrorType> ex)
							{
								if (ex.Detail != null &&
									(ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
									ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
								{
									response = "OK";
									return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
								}
								var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

								WebApiApplication.DbLogger.Error(exp);
								return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
							}
							catch (Exception ex)
							{
								WebApiApplication.DbLogger.Error(ex);
							}
							return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("", Encoding.UTF8) };
						}
					}
				}
            }
        }
    }
}
