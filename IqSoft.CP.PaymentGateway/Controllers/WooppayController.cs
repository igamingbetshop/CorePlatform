using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using System.Net.Http;
using System;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using IqSoft.CP.Integration.WooppayCustomer;
using System.ServiceModel.Channels;
using IqSoft.CP.PaymentGateway.Models.Wooppay;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using System.Web;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class WooppayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Wooppay);
        private static readonly BllPaymentSystem _paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Wooppay);

		[HttpGet]
        [Route("api/Wooppay/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri]int OrderId)
        {
            var jsonResponse = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
					try
					{
						BaseBll.CheckIp(WhitelistedIps);
						var request = paymentSystemBl.GetPaymentRequestById(OrderId);
						if (request == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(request.ClientId.Value);
						if (client == null)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
																						   request.CurrencyId, (int)PaymentRequestTypes.Deposit);

						var loginRequest = new CoreLoginRequest
						{
							username = partnerPaymentSetting.UserName,
							password = partnerPaymentSetting.Password
						};
						var xmlControllerPortTypeClient = new XmlControllerPortTypeClient();
						var loginResponse = xmlControllerPortTypeClient.core_login(loginRequest);

						if (loginResponse.error_code != 0)
							throw new ArgumentNullException(loginResponse.error_code.ToString());

						using (OperationContextScope scope = new OperationContextScope(xmlControllerPortTypeClient.InnerChannel))
						{
							var prop = new HttpRequestMessageProperty();
							prop.Headers.Add(HttpRequestHeader.Cookie, "session=" + loginResponse.response.session);
							OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, prop);

							var input = new CashGetOperationDataRequest
							{
								operationId = new int[] { Convert.ToInt32(request.ExternalTransactionId) }
							};
							var cashGetOperationDataResponse = xmlControllerPortTypeClient.cash_getOperationData(input);

							if (cashGetOperationDataResponse.error_code == 0 && cashGetOperationDataResponse.response.records[0].status == 4)
							{
								if (cashGetOperationDataResponse.response.records[0].sum != (float)request.Amount)
									throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
								clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                var response = new
								{
									data = 1
								};
								jsonResponse = JsonConvert.SerializeObject(response);
							}
						}
					}
					catch (FaultException<BllFnErrorType> fex)
					{
						if (fex.Detail != null &&
							(fex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
							fex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
						{
							var response = new
							{
								data = 1
							};
							jsonResponse = JsonConvert.SerializeObject(response);
						}
						else
						{
							var response = new
							{
								ErrorCode = fex.Detail.Id,
								ErrorMessage = fex.Detail.Message
							};
							jsonResponse = JsonConvert.SerializeObject(response);
						}
					}
					catch (Exception ex)
					{
						var response = new
						{
							ErrorMessage = ex.Message
						};
						jsonResponse = JsonConvert.SerializeObject(response);
					}
                    WebApiApplication.DbLogger.Info(jsonResponse);
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(jsonResponse, Encoding.UTF8)
                    };
                }
            }
        }

        private static CoreLoginResponse Login(string username, string pass)
        {
            var loginRequest = new CoreLoginRequest();
            loginRequest.username = username;
            loginRequest.password = pass;
            var xmlControllerPortTypeClient =
                new XmlControllerPortTypeClient();
            return xmlControllerPortTypeClient.core_login(loginRequest);
        }



        [HttpPost]
        [Route("api/Wooppay/Payment")]
        public HttpResponseMessage Payment(PaymentInput input)
        {
            PaymentOutput response;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
				var client = CacheManager.GetClientById(input.ClientId);
				if (client == null)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
				var paymentRequest = new PaymentRequest();
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        if (input.Amount <= 0)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);

                        if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);

                        using (var scope = CommonFunctions.CreateTransactionScope())
                        {
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, _paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            if (partnerPaymentSetting == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
                            var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.WooppayMerchantKeyword).StringValue;
                            var auth = HttpContext.Current.Request.Headers.Get("Signature");
                            var sign = CommonFunctions.ComputeMd5(input.TransactionId + input.ClientId + input.Amount.ToString() + secretKey);
                            if (sign.ToLower() != auth.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
							
                            paymentRequest = new PaymentRequest
                            {
                                Amount = input.Amount,
                                ClientId = client.Id,
                                CurrencyId = client.CurrencyId,
                                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                                ExternalTransactionId = input.TransactionId
                            };

                            var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info);
                            PaymentHelpers.InvokeMessage("PaymentRequst", request.Id);
                            clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            scope.Complete();
                            PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                            BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            BaseHelpers.BroadcastDepositLimit(info);
                        }
                    }
                }
                response = new PaymentOutput
				{
					ClientId = client.Id,
					Amount = paymentRequest.Amount,
					ExternalTransactionId = input.TransactionId,
					TransactionId = paymentRequest.Id.ToString(),
					CreationTime = DateTime.UtcNow.ToString(),
					Status = paymentRequest.Status
				};
			}
            catch (FaultException<BllFnErrorType> ex)
            {
                response = new PaymentOutput
                {
                    ErrorDescription = ex.Message,
                    CreationTime = ex.Detail.DateTimeInfo.ToString() ?? DateTime.UtcNow.ToString()
                };
                if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                ex.Detail.Id == Constants.Errors.RequestAlreadyPayed ||
                                ex.Detail.Id == Constants.Errors.PaymentRequestAlreadyExists))
                {
                    response.Status = (int)PaymentRequestStates.Approved;
                }
                else
                    response.Status = (int)PaymentRequestStates.Failed;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new PaymentOutput
                {
                    Status = (int)PaymentRequestStates.Failed,
                    ErrorDescription = ex.Message,
                    CreationTime = DateTime.UtcNow.ToString()
                };
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response),
                Encoding.UTF8)
            };
        }
    }
}