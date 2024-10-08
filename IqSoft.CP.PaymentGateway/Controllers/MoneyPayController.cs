// Author: Varsik Harutyunyan
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.MoneyPay;
using System;
using Newtonsoft.Json;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using System.Text.RegularExpressions;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.Integration.Payments.Models.MoneyPay;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MoneyPayController : ApiController
    {
        [HttpPost]
        [Route("api/MoneyPay/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var regionBl = new RegionBll(paymentSystemBl))
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                                if (request == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                if (request.Status != (int)PaymentRequestStates.Pending)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                                var clientSession = paymentSystemBl.GetClientSessionById(request.SessionId ?? 0);

                                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                                input.HolderName = Regex.Replace(input.HolderName, @"\s+", " ");
                                var holderName = input.HolderName.Trim().Split(' ');
                                var firstDigits = input.CardNumber.Substring(0, 6);
                                var lastDigits = input.CardNumber.Substring(input.CardNumber.Length - 4, 4);
                                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(request.Info) ? request.Info : "{}");
                                paymentInfo.CardNumber = string.Concat(firstDigits, new String('*', input.CardNumber.Length - firstDigits.Length - lastDigits.Length), lastDigits);
                                paymentInfo.CardHolderName = input.HolderName;

                                if (!string.IsNullOrEmpty(input.Country))
                                    paymentInfo.Country = input.Country;
                                if (!string.IsNullOrEmpty(input.City))
                                    paymentInfo.City = input.City;
                                request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    DefaultValueHandling = DefaultValueHandling.Ignore
                                });
                                request.CardNumber = paymentInfo.CardNumber;
                                request.CountryCode = paymentInfo.Country;
                                paymentSystemBl.ChangePaymentRequestDetails(request);

                                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MoneyPayApiUrl).StringValue;

                                var merchantInfo = partnerPaymentSetting.Password.Split(',');

                                fnPartnerBankInfo bankInfo = null;
                                if (!string.IsNullOrEmpty(paymentInfo.BankId))
                                {
                                    bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                                }
                                if (bankInfo == null)
                                {
                                    bankInfo = new fnPartnerBankInfo();
                                    bankInfo.BankName = string.Empty;
                                }
                                string channel_id = "";
                                string secret_key = "";
                                if (paymentInfo.CardNumber.StartsWith("34") || paymentInfo.CardNumber.StartsWith("37"))
                                {
                                    channel_id = merchantInfo[1].Split('-')[0];
                                    secret_key = merchantInfo[1].Split('-')[1];
                                }
                                else
                                {
                                    channel_id = merchantInfo[0].Split('-')[0];
                                    secret_key = merchantInfo[0].Split('-')[1];
                                }
                                var signObj = new
                                {
                                    version = "31",
                                    merchant_id = partnerPaymentSetting.UserName,
                                    channel_id,
                                    transaction_type = "PAY",
                                    transaction_id = request.Id.ToString(),
                                    transaction_amount = request.Amount.ToString(),
                                    transaction_currency = request.CurrencyId,
                                    secret_key
                                };

                                var xmlText = CommonFunctions.GetUriDataFromObject(signObj);
                                var sign = CommonFunctions.ComputeMd5(xmlText).ToUpper();
                                var transaction = new
                                {
                                    version = "31",
                                    merchant_id = partnerPaymentSetting.UserName,
                                    channel_id,
                                    transaction_type = "PAY",
                                    transaction_id = request.Id.ToString(),
                                    transaction_amount = request.Amount.ToString(),
                                    transaction_currency = request.CurrencyId,
                                    card_number = paymentInfo.CardNumber,
                                    card_secureid = paymentInfo.ActivationCode,
                                    card_expiry = input.ExpiryMonth + "/" + input.ExpiryYear,
                                    payment_bank = bankInfo.BankName,
                                    customer_firstname = client.FirstName,
                                    customer_lastname = client.LastName,
                                    customer_email = client.Email,
                                    customer_phone = client.MobileNumber,
                                    device_fingerprintid = client.Id,
                                    device_ipaddress = Constants.DefaultIp,
                                    shipping_firstname = client.FirstName,
                                    shipping_lastname = client.LastName,
                                    shipping_phone = client.MobileNumber,
                                    shipping_postalcode = client.Id,
                                    shipping_country = !string.IsNullOrEmpty(input.Country) ? input.Country : string.Empty,
                                    shipping_state = !string.IsNullOrEmpty(input.Country) ? input.Country : string.Empty,
                                    shipping_city = !string.IsNullOrEmpty(input.City) ? input.City : string.Empty,
                                    shipping_address = !string.IsNullOrEmpty(input.Address) ? input.Address : string.Empty,
                                    return_url = input.RedirectUrl,
                                    sign
                                };
                                var xmlDoc = CommonFunctions.GetUriDataFromObject(transaction);
                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                    RequestMethod = Constants.HttpRequestMethods.Post,
                                    Url = url,
                                    PostData = xmlDoc,
                                };
                                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                                var response = JsonConvert.DeserializeObject<PaymentOutput>(resp);

                                if (response.Result_code?.ToUpper() == "FAILED" || response.Result_code?.ToUpper() == "ERROR")
                                {
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, response.Result_code, notificationBl);
                                    throw new Exception(response.Response_message);
                                }
                                request.ExternalTransactionId = response.Transaction_id;
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);

                result.StatusCode = ex.Detail.Id;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                result.StatusCode = Constants.Errors.GeneralException;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8)
            };
        }
    }
}