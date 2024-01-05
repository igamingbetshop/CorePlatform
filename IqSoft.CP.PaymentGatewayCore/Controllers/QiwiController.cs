using System;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Qiwi;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.DAL.Models;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class QiwiController : ControllerBase
    {
		private static readonly List<string> WhitelistedIps = new List<string>
        {
            "89.218.54.34",
            "89.218.54.36"
        };

		[HttpGet]
        [HttpPost]
        [Route("api/Qiwi/ApiRequest")]
        public ActionResult ApiRequest([FromQuery]InputBase input)
        {
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                OutputBase response;
                switch (input.Command)
                {
                    case QiwiHelpers.Methods.Check:
                        response = Check(input);
                        break;
                    case QiwiHelpers.Methods.Pay:
                        response = Pay(input);
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.NotAllowed);
                }
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new OutputBase
                    {
                        Result = QiwiHelpers.GetErrorCode(Constants.Errors.GeneralException),
                        OsmpTxnId = input.Txn_id,
                        Sum = input.Sum,
                        Comment = ex.Message
                    }
                    : new OutputBase
                    {
                        Result = QiwiHelpers.GetErrorCode(ex.Detail.Id),
                        OsmpTxnId = input.Txn_id,
                        Sum = input.Sum,
                        Comment = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new OutputBase
                {
                    Result = QiwiHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    OsmpTxnId = input.Txn_id,
                    Sum = input.Sum,
                    Comment = ex.Message
                };
                return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml));
            }
        }

        private static OutputBase Check(InputBase input)
        {
            using (var clientBll = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                var response = new OutputBase();
                var paymentSystem = CacheManager.GetPaymentSystemByName(string.IsNullOrEmpty(input.BillNo) ? Constants.PaymentSystems.QiWiTerminal : Constants.PaymentSystems.QiWiWallet);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                int clientId;
                if (!int.TryParse(input.Account, out clientId))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongConvertion);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);

                if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);

                if (clientBll.GetClientPaymentSettings(client.Id, (int)ClientPaymentStates.Blocked, false).Any(x => x.PartnerPaymentSettingId == partnerPaymentSetting.Id))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);


                response.Result = (int)QiwiHelpers.ResponseCodes.Success;
                response.OsmpTxnId = input.Txn_id;
                response.Sum = input.Sum;
                return response;
            }
        }

        private static OutputBase Pay(InputBase input)
        {
            Program.DbLogger.Info("comsum = " + input.Commission);
            if (input.Sum <= 0)
				throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);

			int clientId;
			OutputBase response;
			if (!int.TryParse(input.Account, out clientId))
				throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongConvertion);
			var client = CacheManager.GetClientById(clientId);
			if (client == null)
				throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
			var clientState = client.State;
			if (clientState == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
				throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);
            PaymentRequest paymentRequest = null;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
				using (var clientBl = new ClientBll(paymentSystemBl))
				{
					using (var scope = CommonFunctions.CreateTransactionScope())
					{
						var paymentSystem = CacheManager.GetPaymentSystemByName(string.IsNullOrEmpty(input.BillNo) ? Constants.PaymentSystems.QiWiTerminal : Constants.PaymentSystems.QiWiWallet);
						if (paymentSystem == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
						if (partnerPaymentSetting == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
						if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
						paymentRequest = paymentSystemBl.GetPaymentRequest(input.Txn_id, paymentSystem.Id, (int)PaymentRequestTypes.Deposit);

						long paymentRequestId;
						if (!string.IsNullOrEmpty(input.BillNo))
						{
							paymentRequestId = Convert.ToInt64(input.BillNo.Substring(0, input.BillNo.Length / 3));
							paymentRequest = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
							paymentRequest.ExternalTransactionId = input.Txn_id;
							paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						}
						else if (paymentRequest != null)
							paymentRequestId = paymentRequest.Id;
						else
						{
							paymentRequest = new PaymentRequest
							{
								ClientId = clientId,
								Amount = input.Sum / 0.95m,
								CurrencyId = client.CurrencyId,
								PaymentSystemId = paymentSystem.Id,
								PartnerPaymentSettingId = partnerPaymentSetting.Id,
								ExternalTransactionId = input.Txn_id,
								Info = input.Trm_id.ToString()
							};
							paymentRequestId = clientBl.CreateDepositFromPaymentSystem(paymentRequest).Id;
                            PaymentHelpers.InvokeMessage("PaymentRequst", paymentRequestId);
                        }

						clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
						response = new OutputBase
						{
							OsmpTxnId = input.Txn_id,
							Sum = input.Sum,
							Result = Constants.SuccessResponseCode,
							Prvtxn = paymentRequestId.ToString(),
							Comment = QiwiHelpers.Statuses.Success
						};
						scope.Complete();
					}
				}
            }
            return response;
		}
    }
}
