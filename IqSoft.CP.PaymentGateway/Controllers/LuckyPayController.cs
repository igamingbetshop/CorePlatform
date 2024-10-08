using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.LuckyPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class LuckyPayController : ApiController
    {
		private BllPaymentSystem _paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.LuckyPay);
		public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.LuckyPay);

		[HttpPost]
        [Route("api/LuckyPay/Check")]
        public HttpResponseMessage WithdrawResult(PaymentStatusInput input)
        {
            try
            {
				BaseBll.CheckIp(WhitelistedIps);
				var client = CacheManager.GetClientByMobileNumber(input.partnerId, input.mobileNumber);
                if (client.Id > 0)
                {
                    var response = JsonConvert.SerializeObject(new { Id = client.Id, FirstName = client.FirstName, LastName = client.LastName, CurrencyId = client.CurrencyId });
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent (response, Encoding.UTF8) };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            }
        }

		[HttpPost]
		[Route("api/LuckyPay/Deposit")]
		public HttpResponseMessage ApiRequest(PaymentStatusInput input)
		{
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
				if (input.amount <= 0)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
				var client = CacheManager.GetClientById(input.clientId);
				if (client == null)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
				var clientState = client.State;
				if (clientState == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);
				PaymentRequest paymentRequest = null;
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var scope = CommonFunctions.CreateTransactionScope())
						{
							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, _paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
							if (partnerPaymentSetting == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
							if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
							paymentRequest = paymentSystemBl.GetPaymentRequest(input.transactionId.ToString(), _paymentSystem.Id, (int)PaymentRequestTypes.Deposit);

							if(paymentRequest != null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestAlreadyExists);
							
							paymentRequest = clientBl.CreateDepositFromPaymentSystem(new PaymentRequest
							{
								ClientId = client.Id,
								Amount = input.amount,
								CurrencyId = client.CurrencyId,
								PaymentSystemId = _paymentSystem.Id,
								PartnerPaymentSettingId = partnerPaymentSetting.Id,
								ExternalTransactionId = input.transactionId.ToString(),
								Info = input.transactionId.ToString()
							}, out LimitInfo info);
							PaymentHelpers.InvokeMessage("PaymentRequst", paymentRequest.ClientId.Value);
							clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            scope.Complete();
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastDepositLimit(info);
						}
					}
				}
				var response = new 
				{
					ClientId = input.clientId,
					TransactionId = paymentRequest.Id,
					Amount = input.amount
				};
				return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8) };
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				WebApiApplication.DbLogger.Error(ex);
				return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(ex);
				return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
			}
		}
    }
}
