using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.BRPay;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class BRPayHelpers
    {
        public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId.Value);
                        if (string.IsNullOrEmpty(client.MobileNumber))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);

                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BRPay);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.BRPayUrl).StringValue;
                        var ps = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.BRPayPaymentSystem).StringValue;                        
                        var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                        var outletId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.BRPayOutletId).NumericValue;
                        var data = new PaymentInput()
                        {
                            Amount = input.Amount,
                            Description = "BRPay",
                            FailureUrl = cashierPageUrl,
                            OrderId = input.Id,
                            OutletId = Convert.ToInt64(outletId.Value),
                            PaymentSystem = ps,
                            ResultUrl = string.Format("{0}/api/BRPay/ApiRequest", paymentGateway),
                            Salt = CommonFunctions.GetRandomString(16),
                            SuccessUrl = cashierPageUrl,
                            Email = client.Email,
                            UserIp = string.IsNullOrEmpty(session.LoginIp) ? Constants.DefaultIp : session.LoginIp,
                            UserName = client.UserName,
                            UserPhone = client.MobileNumber.Replace("+", "")
                        };

                        var orderdParams = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}", data.Amount, data.Description,
                                                   data.FailureUrl, data.OrderId, data.OutletId, data.PaymentSystem, data.ResultUrl, data.Salt, 
                                                   data.SuccessUrl, data.Email, data.UserIp, data.UserName, data.UserPhone);
                        data.Signature = CommonFunctions.ComputeMd5("init_payment;" + orderdParams + ";" + partnerPaymentSetting.UserName);                        
                        
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format("{0}{1}", url, "init_payment"),
                            PostData = "sp_json=" + JsonConvert.SerializeObject(data),
                        };
                        var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        var output = JsonConvert.DeserializeObject<PaymentOutput>(response);

                        if (output.Status == "ok")
                        {
                            input.ExternalTransactionId = output.PaymentId;
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            return output.RedirectUrl;
                        }
                        else
                        {
                            throw new Exception($"Error: {output.ErrorCode} {output.ErrorDescription}");
                        }
                    }
                }
            }
        }

        public static PaymentResponse PayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var regionBl = new RegionBll(clientBl))
                        {
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BRPay);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                            var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.BRPayUrl);
                            var outletId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.BRPayOutletId).NumericValue;
                            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                            var data = new PayoutInput()
                            {
                                Amount = amount,
                                OutletId = Convert.ToInt64(outletId.Value),
                                DestinationCard = paymentInfo.WalletNumber,
                                OrderId = paymentRequest.Id,
                                Salt = CommonFunctions.GetRandomString(16),
                            };
                            var orderdParams = string.Format("{0};{1};{2};{3};{4}", data.Amount, data.DestinationCard, data.OrderId,
                                                                                    data.OutletId, data.Salt);
                            data.Signature = CommonFunctions.ComputeMd5("transfer_to_card_rus;" + orderdParams + ";" + partnerPaymentSetting.UserName);

                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format("{0}{1}", url, "transfer_to_card_rus"),
                                PostData = "sp_json=" + JsonConvert.SerializeObject(data),
                            };
                            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
                            if (output.Status == "ok")
                            {
                                paymentRequest.ExternalTransactionId = output.TransactionId;
                                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.PayPanding,
                                };
                            }
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.Failed,
                                Description = output.ErrorMessage
                            };
                        }
                    }
                }
            }
        }
    }
}

