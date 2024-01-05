using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.TronLink;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class TronLinkHelpers
    {
        public static PaymentResponse PayVoucher(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TronLinkUrl).StringValue;

            var requestInput = new
            {
                id = paymentInfo.ActivationCode,
                hash = paymentInfo.VoucherNumber
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", string.Format("Bearer {0}", partnerPaymentSetting.Password) } },
                Url = string.Format("{0}/api/transaction?{1}", url, CommonFunctions.GetUriDataFromObject(requestInput))
            };
            log.Info("Request_" + JsonConvert.SerializeObject(httpRequestInput));
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var result = JsonConvert.DeserializeObject<PaymentOutput>(resp);
            log.Info("Response_" + resp);

            using var clientBl = new ClientBll(session, log);
            using var notificationBl = new NotificationBll(clientBl);
            if (result.data.success)
            {
                var amount = result.data.amount;
                input.ExternalTransactionId = paymentInfo.VoucherNumber;
                if (input.CurrencyId != Constants.Currencies.USADollar)
                {
                    var rate = BaseBll.GetCurrenciesDifference(Constants.Currencies.USADollar, client.CurrencyId);
                    amount *= rate;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.USADollar);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                }
                input.Amount = Math.Floor(amount * 100) / 100;

                using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                {

                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    clientBl.ApproveDepositFromPaymentSystem(input, false);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved,
                    };
                }
            }
            else
                clientBl.ChangeDepositRequestState(input.Id, PaymentRequestStates.Failed, string.Empty, notificationBl);

            return new PaymentResponse
            {
                Status = PaymentRequestStates.Failed,
                Description = "FAILD"
            };
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var client = CacheManager.GetClientById(input.ClientId);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TronLinkUrl).StringValue;
                                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                                if (!Enum.IsDefined(typeof(CryptoTypes), Convert.ToInt32(paymentInfo.AccountType)))
                                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                                var amount = input.Amount - (input.CommissionAmount ?? 0);
                                if (input.CurrencyId != Constants.Currencies.USADollar)
                                {
                                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar);
                                    amount *= rate;
                                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                                    parameters.Add("Currency", Constants.Currencies.USADollar);
                                    parameters.Add("AppliedRate", rate.ToString("F") );
                                    input.Parameters = JsonConvert.SerializeObject(parameters);
                                }
                                var paymentRequestInput = new
                                {
                                    id = input.Id,
                                    address = paymentInfo.WalletNumber,
                                    amount = Math.Floor(amount * 100) / 100
                                };

                                var path = paymentInfo.AccountType = (paymentInfo.AccountType == ((int)CryptoTypes.Tether).ToString()) ? "transfer-usdt" : "transfer-trx";
                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                                    RequestMethod = System.Net.Http.HttpMethod.Post,
                                    RequestHeaders = new Dictionary<string, string> { { "Authorization", string.Format("Bearer {0}", partnerPaymentSetting.Password) } },
                                    Url = string.Format("{0}/api/{1}", url, path),
                                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                                };
                                var response = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                                if (response.success)
                                {
                                    input.ExternalTransactionId = response.data.tx;
                                    paymentSystemBl.ChangePaymentRequestDetails(input);
                                    var resp = clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Approved, string.Empty, null, null, 
                                                                                   false, string.Empty, documentBl, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    return new PaymentResponse
                                    {
                                        Status = PaymentRequestStates.Approved
                                    };
                                }
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.Failed,
                                    Description = string.Format("{0}  {1}", response.error.status, response.error.message)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                    Description = ex.Message
                };
            }
        }

        private enum CryptoTypes
        {
            Trx = 1,
            Tether = 2
        }
    }
}
