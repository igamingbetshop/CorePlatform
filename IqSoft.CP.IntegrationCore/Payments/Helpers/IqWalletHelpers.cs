using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Extensions;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.IqWallet;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class IqWalletHelpers
    {
        public static PaymentResponse CallIqWalletApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    if (string.IsNullOrEmpty(client.MobileNumber) || !client.IsMobileNumberVerified)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberNotVerified);

					var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    if (paymentsystem == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.IqWalletUrl).StringValue;
                    var clientModel = clientBl.GetClientById(input.ClientId);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var paymentRequestInput = new PaymentInput
                    {
                        MerchantId = Convert.ToInt32(partnerPaymentSetting.UserName),
                        MerchantPaymentId = input.Id,
                        MobileNumber = client.MobileNumber,
                        Currency = input.CurrencyId,
                        Amount = string.Format("{0:N2}", input.Amount),
                        MerchantClient = null
                    };

					paymentRequestInput.Sign = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ",") + partnerPaymentSetting.Password);
					
					paymentRequestInput.MerchantClient = clientModel.ToBllClient();
					var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = System.Net.Http.HttpMethod.Post,
                        Url = string.Format("{0}/PaymentRequest", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    input.ExternalTransactionId = response.PaymentId.ToString();
                    paymentSystemBl.ChangePaymentRequestDetails(input);

					if (response.ErrorCode == Constants.SuccessResponseCode && response.Status == (int)PaymentRequestStates.Approved)
                    {
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Confirmed,
                        };
                    }
                    return new PaymentResponse
					{	
                        Status = PaymentRequestStates.Failed,
                        Description = response.ErrorDescription
                    };
                }
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    if (!client.IsMobileNumberVerified)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberNotVerified);

                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                        input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.IqWalletUrl).StringValue;
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var clientModel = clientBl.GetClientById(input.ClientId);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    var paymentRequestInput = new PaymentInput
                    {
                        MerchantId = Convert.ToInt32(partnerPaymentSetting.UserName),
                        MerchantPaymentId = input.Id,
                        MobileNumber = client.MobileNumber,
                        Currency = input.CurrencyId,
                        Amount = string.Format("{0:N2}", amount),
                        MerchantClient = null
                    };
                    paymentRequestInput.Sign = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ",") +
                                                                             partnerPaymentSetting.Password);
                    paymentRequestInput.MerchantClient = clientModel.ToBllClient();
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = System.Net.Http.HttpMethod.Post,
                        Url = string.Format("{0}/PayoutRequest", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    input.ExternalTransactionId = response.PaymentId.ToString();
                    paymentSystemBl.ChangePaymentRequestDetails(input);

					if (response.ErrorCode == Constants.SuccessResponseCode && response.Status == (int)PaymentRequestStates.Approved)
                    {
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
							Description = "Pay Panding"
						};
                    }
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = response.ErrorDescription
                    };
                }
            }
        }
    }
}
