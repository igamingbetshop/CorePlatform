using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Interkassa;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
	public class InterkassaHelpers
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
						if (string.IsNullOrWhiteSpace(client.Email))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
						if (string.IsNullOrWhiteSpace(client.FirstName))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
						if (string.IsNullOrWhiteSpace(client.LastName))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
						var partner = CacheManager.GetPartnerById(client.PartnerId);
                        var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
						var paymentMethod = CacheManager.GetPartnerSettingByKey(client.PartnerId, paymentSystem.Name).StringValue.Split(',');
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaUrl).StringValue;
                        var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                        var checkout = partnerPaymentSetting.UserName.Split(',');
						var data = new
						{
							ik_co_id = checkout[0],
							ik_cur = checkout[1],
							ik_am = input.Amount,
							ik_pm_no = input.Id.ToString(),
							ik_desc = "Deposit", //partner.Name,
							ik_act = "process",
							ik_int = "json",
							ik_payment_method = paymentMethod[0],
							ik_payment_currency = paymentMethod[1],
							ik_ia_u = string.Format("{0}/api/Interkassa/ApiRequest", paymentGateway),
							ik_ia_m = Constants.HttpRequestMethods.Post,
							//ik_suc_u = cashierPageUrl,
							ik_customer_first_name = client.FirstName,
							ik_customer_last_name = client.LastName,
							ik_customer_email = client.Email
						};
                        var orderdParams = CommonFunctions.GetSortedValuesAsString(data, ":");
						using (SHA256 sha256Hash = SHA256.Create())
                        {
                            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(orderdParams + ":" + partnerPaymentSetting.Password));
                            var signature = Uri.EscapeDataString(Convert.ToBase64String(bytes));
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format(url, "sci"),
                                PostData = CommonFunctions.GetUriEndocingFromObject(data) + $"&ik_sign={signature}"
                            };
                            log.Info(JsonConvert.SerializeObject(httpRequestInput));

                            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
                            if (output.ResultMsg == "Success")
                            {
                                return output.ResultData.PaymentForm.Action;
                            }
                            else
                            {
                                throw new Exception($"Error: {output.ResultCode} {output.ResultMsg}");
                            }
                        }
                    }
                }
            }
        }

        public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId.Value);
                        if (string.IsNullOrEmpty(client.FirstName))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                        if (string.IsNullOrEmpty(client.LastName))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                        if (string.IsNullOrWhiteSpace(client.MobileNumber))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                        if (string.IsNullOrWhiteSpace(client.Email))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                        var accountId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaAccountId).StringValue;
                        var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}");
						var amount = input.Amount - (input.CommissionAmount ?? 0);
						var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
						var paymentMethod = CacheManager.GetPartnerSettingByKey(client.PartnerId, paymentSystem.Name).StringValue.Split(',');
						var headers = new Dictionary<string, string>
                        {
                            { "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
                            { "Ik-Api-Account-Id", accountId}
                        };
						var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
						string bankName = null;
						string iban = null;
						string card = paymentInfo.CardNumber;
						var accountNumber = paymentInfo.WalletNumber;
                        var documentId = paymentInfo.DocumentId;
						if (paymentSystem.Name == Constants.PaymentSystems.InterkassaPapara)
							accountNumber = paymentInfo.AccountNumber;
                        if (paymentSystem.Name == Constants.PaymentSystems.InterkassaTrBanking)
						{
							bankName = paymentInfo.BankName;
							documentId = paymentInfo.CardNumber;
							accountNumber = paymentInfo.BankAccountNumber;
							iban = paymentInfo.BankACH;
                            card = null;
						}
						var payoutInput = new PayoutInput()
                        {
                            amount = amount,
                            method = paymentMethod[0],
                            currency = paymentMethod[1],
                            useShortAlias = true,
                            purseId = paymentMethod[2],
                            action = "process",
							calcKey = "ikPayerPrice",
                            paymentNo = input.Id
                        };
                        var paymentDetails = new Dictionary<string, string>()
                        {
                            { "first_name", client.FirstName },
                            { "last_name", client.LastName},
                            { "email", client.Email},
							{ "account_number", accountNumber },
							{ "phone", client.MobileNumber.Replace("+", string.Empty)},
							{ "document_id", documentId },
                            { "iban", iban },
                            { "bank_identity", bankName },
							{ "card", card }
						};
						var details = string.Empty;
						foreach (var detail in paymentDetails)
						{
							if (!string.IsNullOrEmpty(detail.Value))
							{
								var item = $"&details[{detail.Key}]={detail.Value}";
								details = details + item;
							}
						}
                        var postData = CommonFunctions.GetUriDataFromObject<PayoutInput>(payoutInput) + details;
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaUrl).StringValue;
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            RequestHeaders = headers,
                            Url = string.Format(url, "api") + "/v1/withdraw",
                            PostData = postData 
                        };
						log.Info(string.Format("PostData: {0}", postData));
						var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info(string.Format("Message: {0}", response));
                        var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
                        if (output.Status == "ok")
                        {
                            var data = JsonConvert.DeserializeObject<Data>(JsonConvert.SerializeObject(output.Data));
                            input.ExternalTransactionId = data?.Transaction?.OpId;
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.PayPanding,
                            };
                        }
                        else
                            throw new Exception(string.Format("ErrorMessage: {0}", output.Message));
                    }
                }
            }
        }


        public static List<int> GetTransactionDetails(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var userIds = new List<int>();
            using (var paymentSystemBl = new PaymentSystemBll(session, log))            
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaUrl).StringValue;
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var accountId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaAccountId).StringValue;
                var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}");
                var headers = new Dictionary<string, string>
                {
                    { "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
                    { "Ik-Api-Account-Id", accountId}
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    RequestHeaders = headers,
                    Url = string.Format(url, "api") + $"/v1/withdraw/{input.ExternalTransactionId}",
                };
                var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = JsonConvert.DeserializeObject<PayoutOutput>(res);

                using (var clientBl = new ClientBll(session, log))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            if (output.Status == "ok")
                            {
                                var data = JsonConvert.DeserializeObject<PayoutResult>(JsonConvert.SerializeObject(output.Data));
                                if (data.stateName == "success")
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Approved,
                                        string.Empty, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                }
                                else if(data.stateName == "canceled")
                                    clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Failed,
											null, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                            }
                        }
                    }
                }
            }
            return userIds;
        }
    }
}

