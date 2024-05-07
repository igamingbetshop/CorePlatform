// functionality not using 

using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.IqWalletWebApi.Models.IqWallet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.IqWalletWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class IqWalletController : ApiController
    {
        private static List<string> WhitelistedIps = new List<string>
        {
            "??"
        };

        [HttpPost]
        [Route("api/IqWallet/PaymentRequest")]
        public HttpResponseMessage PaymentRequest(PaymentRequestInput input)
        {
            var response = new PaymentRequestOutput();

			using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var documentBl = new DocumentBll(clientBl))
				{
					using (var notificationBl = new NotificationBll(clientBl))
					{
						try
						{
							//BaseBll.CheckIp(WhitelistedIps);
							var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.IqWallet);
							if (paymentSystem == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
							var merchant = CacheManager.GetMerchantById(input.MerchantId);
							if (merchant == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
							var partner = CacheManager.GetPartnerById(merchant.PartnerId);
							var client = CacheManager.GetClientByMobileNumber(merchant.PartnerId, input.MobileNumber);

							if (client == null)
							{
								input.MerchantClient.PartnerId = merchant.PartnerId;
								input.MerchantClient.UserName = input.MerchantClient.UserName + "_" + partner.Name + "_" + partner.Id;
								input.MerchantClient.CurrencyId = partner.CurrencyId;
								clientBl.InsertClient(input.MerchantClient);
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.LowBalance);
							}
							input.MerchantClient = null;
							var sign = input.Sign.ToLower();
							input.Sign = null;
							var signature = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(input, ",") + merchant.MerchantKey);

							if (signature.ToLower() != sign)
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
							if (partnerPaymentSetting == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

							var payoutRequest = new PaymentRequestModel
							{
								PartnerId = merchant.PartnerId,
								ClientId = client.Id,
								Amount = BaseBll.ConvertCurrency(input.Currency, client.CurrencyId, Convert.ToDecimal(input.Amount)),
								CurrencyId = client.CurrencyId,
								PaymentSystemId = paymentSystem.Id
							};
							var request = clientBl.CreateWithdrawPaymentRequest(payoutRequest, 0, client, documentBl, notificationBl);//??
							request.ExternalTransactionId = input.MerchantPaymentId.ToString();
							var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, 
								string.Empty, null, null, true, request.Parameters, documentBl, notificationBl);

							response.Status = (int)PaymentRequestStates.Approved;
							response.Amount = string.Format("{0:N2}", input.Amount);
							response.PaymentId = request.Id;
							response.MerchantPaymentId = input.MerchantPaymentId;
							response.Sign = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(response, ",") + merchant.MerchantKey);

							var merchantRequest = new MerchantRequest
							{
								RequestUrl = string.Format("{0}/PaymentRequest", merchant.MerchantUrl),
								Content = JsonConvert.SerializeObject(response),
								Status = (int)MerchantRequestStates.Active,
								Response = string.Empty,
								RetryCount = 0
							};
							var doc = clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl, merchantRequest);
						}
						catch (FaultException<BllFnErrorType> ex)
						{
							if (ex.Detail != null &&
								(ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
								ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
							{
								response.Status = (int)PaymentRequestStates.Approved;
								response.ErrorCode = Constants.SuccessResponseCode;
							}
							else
							{
								response.Status = (int)PaymentRequestStates.Failed;
								var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
								response.ErrorCode = ex.Detail.Id;
								response.ErrorDescription = exp.Message;
								WebApiApplication.DbLogger.Error(exp);
							}
						}
						catch (Exception ex)
						{
							WebApiApplication.DbLogger.Error(ex);
							response.ErrorCode = Constants.Errors.GeneralException;
							response.ErrorDescription = ex.Message;
							response.Status = (int)PaymentRequestStates.Failed;
						}
						return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8) };
					}
				}
			}
        }

        [HttpPost]
        [Route("api/IqWallet/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PaymentRequestInput input)
        {
            var response = new PaymentRequestOutput();

			using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var paymentSystemBll = new PaymentSystemBll(clientBl))
				{
					try
					{
						WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
						//BaseBll.CheckIp(WhitelistedIps);
						var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.IqWallet);
						if (paymentSystem == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
						var merchant = CacheManager.GetMerchantById(input.MerchantId);
						if (merchant == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
						var partner = CacheManager.GetPartnerById(merchant.PartnerId);
						var client = CacheManager.GetClientByMobileNumber(merchant.PartnerId, input.MobileNumber);
						if (client == null)
						{
							input.MerchantClient.PartnerId = merchant.PartnerId;
							input.MerchantClient.UserName = input.MerchantClient.UserName + "_" + partner.Name + "_" + partner.Id;
							input.MerchantClient.CurrencyId = partner.CurrencyId;
							clientBl.InsertClient(input.MerchantClient);
							client = CacheManager.GetClientByMobileNumber(merchant.PartnerId, input.MobileNumber);
						}
						input.MerchantClient = null;
						var sign = input.Sign.ToLower();
						input.Sign = null;
						var signature = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(input, ",") + merchant.MerchantKey);

						if (signature.ToLower() != sign)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
													client.CurrencyId, (int)PaymentRequestTypes.Deposit);
						if (partnerPaymentSetting == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
						
						var inputAmount = Convert.ToDecimal(input.Amount);

						var paymentRequest = new PaymentRequest
						{
							Amount = BaseBll.ConvertCurrency(input.Currency, client.CurrencyId, Convert.ToDecimal(input.Amount)),
							ClientId = client.Id,
							CurrencyId = client.CurrencyId,
							PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
							PartnerPaymentSettingId = partnerPaymentSetting.Id,
							ExternalTransactionId = input.MerchantPaymentId.ToString()
						};
						var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info);

						response.Status = (int)PaymentRequestStates.Approved;
						response.Amount = string.Format("{0:N2}", inputAmount);
						response.PaymentId = request.Id;
						response.MerchantPaymentId = input.MerchantPaymentId;
						response.Sign = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(response, ",") + merchant.MerchantKey);

						var merchantRequest = new MerchantRequest
						{
							RequestUrl = string.Format("{0}/PayoutRequest", merchant.MerchantUrl),
							Content = JsonConvert.SerializeObject(response),
							Status = (int)MerchantRequestStates.Active,
							Response = string.Empty,
							RetryCount = 0
						};
						clientBl.ApproveDepositFromPaymentSystem(request, false, mr: merchantRequest);
					}
					catch (FaultException<BllFnErrorType> ex)
					{
						if (ex.Detail != null &&
							(ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
							ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
						{
							response.Status = (int)PaymentRequestStates.Approved;
							response.ErrorCode = Constants.SuccessResponseCode;
						}
						else
						{
							response.Status = (int)PaymentRequestStates.Failed;
							response.ErrorCode = ex.Detail.Id;
							var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
							response.ErrorDescription = exp.Message;
							WebApiApplication.DbLogger.Error(exp);
						}
					}
					catch (Exception ex)
					{
						WebApiApplication.DbLogger.Error(ex);
						response.ErrorCode = Constants.Errors.GeneralException;
						response.ErrorDescription = ex.Message;
						response.Status = (int)PaymentRequestStates.Failed;
					}
					return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8) };
				}
			}
        }
    }
}
