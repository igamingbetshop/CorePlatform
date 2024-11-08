﻿using IqSoft.CP.BLL.Caching;
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
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.PaymentIQ;
using System;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PaymentIQHelpers
    {
        public static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.PaymentIQ, string.Empty },
            { Constants.PaymentSystems.PaymentIQLuxon, "luxonpay" },
            { Constants.PaymentSystems.PaymentIQSirenPayCard, "creditcard" }, // "USD, JPY"
            { Constants.PaymentSystems.PaymentIQSirenPayWebDirect, "webredirect" }, // "USD, JPY"
            { Constants.PaymentSystems.PaymentIQNeteller, "neteller" },
            { Constants.PaymentSystems.PaymentIQSkrill, "skrill" },
            { Constants.PaymentSystems.PaymentIQCryptoPay, "cryptocurrency" },
            { Constants.PaymentSystems.PaymentIQInterac, "interac" },
            { Constants.PaymentSystems.PaymentIQJeton, "jeton" },
            { Constants.PaymentSystems.PaymentIQPaynetEasyCreditCard, "creditcard" },
            { Constants.PaymentSystems.PaymentIQPaynetEasyWebRedirect, "webredirect" },
            { Constants.PaymentSystems.PaymentIQPaynetEasyBank, "bankdomestic" },
            { Constants.PaymentSystems.PaymentIQInternalCash, "bank" },
            { Constants.PaymentSystems.PaymentIQHelp2Pay, "bankdomestic" },
            { Constants.PaymentSystems.PaymentIQFlexepin, "voucher" }
        };

        public static string CallPaymentIQApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, input.Type);
            var environment = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.PaymentIQEnvironment);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (!PaymentWays.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MethodNotFound);

            if(paymentSystem.Name == Constants.PaymentSystems.PaymentIQSirenPayCard)
            {
                if(string.IsNullOrEmpty( client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(client.ZipCode.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            }
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var paymentProcessingInput = new
            {
                MerchantId = partnerPaymentSetting.UserName,
                UserId = client.Id,
                SessionId = input.Id.ToString(),
                Environment = string.IsNullOrEmpty(environment) ? "test" : environment,
                Method = input.Type == (int)PaymentRequestTypes.Deposit ? "deposit" : "withdrawal",
                input.Amount,
                PartnerName = partner.Name,
                Lang = session.LanguageId,
                PaymentRequestId = input.Id,
                ProviderType​ = PaymentWays[paymentSystem.Name],
                Cashier = cashierPageUrl
            };

            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

            var returnUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
            return string.Format("{0}/paymentiq/paymentprocessing?{1}", returnUrl, CommonFunctions.GetUriEndocingFromObject(paymentProcessingInput));
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                    if (!PaymentWays.ContainsKey(paymentSystem.Name))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MethodNotFound);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                    var accountNumber = paymentInfo.WalletNumber;
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.PaymentIQPayoutApiUrl);
                    var boKey = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.PaymentIQBoKey);
                    var apiClient = partnerPaymentSetting.Password.Split(',');
                    var authTokenOutput = GetAuthToken(client.PartnerId, paymentRequest.PaymentSystemId, apiClient[0], apiClient[1], boKey);
                    if (paymentSystem.Name == Constants.PaymentSystems.PaymentIQLuxon || paymentSystem.Name == Constants.PaymentSystems.PaymentIQSirenPayCard)
                    {
                        var lastDeposit = clientBl.GetClientLastDepositWithParams(paymentSystem.Id, client.Id);
                        if (lastDeposit != null)
                        {
                            var transactionDetails = GetTransactionDetatils(client.PartnerId, paymentRequest.PaymentSystemId, authTokenOutput.TokenType + " " + authTokenOutput.AccessToken,
                                                                            partnerPaymentSetting.UserName, lastDeposit.ExternalTransactionId);
                            if (transactionDetails.MerchantTxId == lastDeposit.Id.ToString() && transactionDetails.MerchantUserId == client.Id.ToString())
                                accountNumber = transactionDetails.MaskedUserAccount;
                        }
                    }
                    var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                    var ExpirationDate = !string.IsNullOrEmpty(paymentInfo.ExpirationDate) ? Convert.ToDateTime(paymentInfo.ExpirationDate) : DateTime.MinValue;

                    var proccessPayoutInput = new
                    {
                        sessionId = paymentRequest.Id,
                        userId = client.Id.ToString(),
                        merchantId = partnerPaymentSetting.UserName,
                        amount = amount.ToString("F"),
                        recipientWalletId = accountNumber,
                        phoneNumber = paymentInfo.MobileNumber,
                        email = paymentInfo.Email,
                        mobile = paymentInfo.MobileNumber,
                        cardHolder = paymentInfo.CardHolderName,
                        encCreditcardNumber = paymentInfo.CardNumber,
                        creditcard = new { cardnumber = paymentInfo.CardNumber },
                        expiryMonth = ExpirationDate != DateTime.MinValue ? ExpirationDate.Month.ToString("d2") : null,
                        expiryYear = ExpirationDate != DateTime.MinValue ? ExpirationDate.Year : (int?)null,
                        walletAddress = paymentInfo.WalletNumber,
                        customerNumber = paymentInfo.WalletNumber,
                        cryptoCurrency = paymentInfo.AccountType,
                        accountNumber = paymentInfo.BankAccountNumber,
                        beneficiaryName = paymentInfo.BeneficiaryName,
                        bankName = paymentInfo.BankName,
                        branchName = paymentInfo.BankBranchName,
                        attributes = new
                        {
                            merchantTxId = paymentRequest.Id.ToString()
                        }
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}api/{1}/withdrawal/process", url, PaymentWays[paymentSystem.Name]),
                        RequestHeaders = new Dictionary<string, string> { { "PIQ-Client-IP", paymentInfo.TransactionIp } },
                        PostData = JsonConvert.SerializeObject(proccessPayoutInput, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        })
                    };
                    var response = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if ((response.Success && (response.StatusCode.ToUpper() == "SUCCESS" ||
                         response.StatusCode.ToUpper() == "WAITING_WITHDRAWAL_APPROVAL" ||
                         response.StatusCode.ToUpper() == "SUCCESS_WAITING_CONFIRMATION")) ||
                        (!response.Success && response.StatusCode.ToUpper() == "WAITING_NOTIFICATION"))
                    {
                        paymentRequest.ExternalTransactionId = response.TxId;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        var approveRequestInput = new
                        {
                            info = "Approve"
                        };
                        httpRequestInput.RequestHeaders = new Dictionary<string, string> { { "Authorization", authTokenOutput.TokenType + " " + authTokenOutput.AccessToken } };
                        httpRequestInput.Url = string.Format("{0}admin/v1/payments/approve/{1}?merchantId={2}", url, response.TxId, partnerPaymentSetting.UserName);
                        httpRequestInput.PostData = JsonConvert.SerializeObject(approveRequestInput);
                        var r = JsonConvert.DeserializeObject<ApproveOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        if (r.State.ToUpper() == "SUCCESSFUL" &&
                           (r.StatusCode.ToUpper() == "SUCCESS_WITHDRAWAL_APPROVAL" || r.StatusCode.ToUpper() == "SUCCESS_WAITING_CONFIRMATION"))
                        {
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.PayPanding
                            };
                        }
                        throw new Exception(r.StatusCode);
                    }

                    throw new Exception(string.Format("StatusCode: {0}, Errors: {1}", JsonConvert.SerializeObject(response.StatusCode),
                                                                                      JsonConvert.SerializeObject(response.Errors)));
                }
            }
        }

        public static PaymentResponse PayPayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log, out List<int> userIds)
        {
            userIds = new List<int>();
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                               paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.PayPanding, string.Empty,
                                                                null, null, true, paymentRequest.Parameters, documentBl, notificationBl, out userIds, false);

                            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.PaymentIQPayoutApiUrl);
                            var boKey = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.PaymentIQBoKey);
                            var apiClient = partnerPaymentSetting.Password.Split(',');
                            var authTokenOutput = GetAuthToken(client.PartnerId, paymentRequest.PaymentSystemId, apiClient[0], apiClient[1], boKey);
                            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                            var requestHeaders = new Dictionary<string, string> { { "Authorization", authTokenOutput.TokenType + " " + authTokenOutput.AccessToken } };
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationJson,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format("{0}admin/v1/payments/approve/{1}?merchantId={2}", url, paymentRequest.ExternalTransactionId, partnerPaymentSetting.UserName),
                                RequestHeaders = requestHeaders,
                                PostData = JsonConvert.SerializeObject(new { info = "Approve" })
                            };
                            var r = JsonConvert.DeserializeObject<ApproveOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                            if (r.State.ToUpper() == "SUCCESSFUL" &&
                               (r.StatusCode.ToUpper() == "SUCCESS_WITHDRAWAL_APPROVAL" || r.StatusCode.ToUpper() == "SUCCESS_WAITING_CONFIRMATION"))
                            {
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.PayPanding
                                };
                            }
                            throw new Exception(r.StatusCode);
                        }
                    }
                }
            }
        }

        public static PaymentRequestStates CancelPayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            if (paymentRequest.Type != (int)PaymentRequestTypes.Withdraw)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                               paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.PaymentIQPayoutApiUrl);
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
            var sessionId = Convert.ToInt64(parameters["SessionId"]);
            //api/user/transaction/{merchantId}/{userId}/{transactionId}?sessionId={sessionId}
            var clientSession = CacheManager.GetClientPlatformSessionById(sessionId);
            url = $"{url}api/user/transaction/{partnerPaymentSetting.UserName}/{client.Id}/{paymentRequest.ExternalTransactionId}?" +
                  $"sessionId={clientSession.Token}";
            log.Info("PIQ URL: " + url);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Delete,
                Url = url,
                PostData = "{}"
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var cancelTransactionResult = JsonConvert.DeserializeObject<CancelTransactionResult>(response);
            if (!cancelTransactionResult.Success)
                throw new Exception(response);
            return PaymentRequestStates.CancelPending;
        }

        private static TransactionDetatils GetTransactionDetatils(int partnerId, int paymentSystemId, string authToken, string merchantId, string txId)
        {
            var url = CacheManager.GetPartnerPaymentSystemByKey(partnerId, paymentSystemId, Constants.PartnerKeys.PaymentIQPayoutApiUrl);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}admin/v1/payments?merchantId={1}&id={2}", url, merchantId, txId),
                RequestHeaders = new Dictionary<string, string> { { "Authorization", authToken } }
            };
            return JsonConvert.DeserializeObject<List<TransactionDetatils>>(CommonFunctions.SendHttpRequest(httpRequestInput, out _))[0];
        }

        private static AuthTokenOutput GetAuthToken(int partnerId, int paymentSystemId, string clientId, string clientSecret, string boKey)
        {
            var url = CacheManager.GetPartnerPaymentSystemByKey(partnerId, paymentSystemId, Constants.PartnerKeys.PaymentIQPayoutApiUrl);
            var byteArray = Encoding.Default.GetBytes(string.Format("{0}:{1}", clientId, clientSecret));
            var headers = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } };
            var boCredentials = AESEncryptHelper.DecryptDistributionString(boKey).Split(',');
            var requestInput = new 
            {
                grant_type = "client_credentials",
                username = boCredentials[0],
                password = boCredentials[1],
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders =headers,
                Url = string.Format("{0}oauth/token?{1}", url, CommonFunctions.GetUriDataFromObject(requestInput)),
                PostData = CommonFunctions.GetUriDataFromObject(requestInput)
            };
            return JsonConvert.DeserializeObject<AuthTokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }
    }
}