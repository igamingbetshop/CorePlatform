using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using System.IO;
using System.Xml;
using IqSoft.CP.Common.Enums;
using System.Web;
using Transaction = IqSoft.CP.Integration.Payments.Models.ApcoPay.Transaction;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Payments.Models.ApcoPay;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class ApcoPayHelpers
    {
        private static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.ApcoPayVisaMaster, "CARDS" },
            { Constants.PaymentSystems.ApcoPayMuchBetter, "MUCHBETTER" },
            { Constants.PaymentSystems.ApcoPayNeoSurf, "NEOSURF" },
            { Constants.PaymentSystems.ApcoPaySafetyPayV2, "SAFETYPAYV2" },
            { Constants.PaymentSystems.ApcoPayBankTransfer, "APBT" },
            { Constants.PaymentSystems.ApcoPayCashPayment, "APCP" },
            { Constants.PaymentSystems.ApcoPayAstroPay, "ASTROPAYCARD" },
            { Constants.PaymentSystems.ApcoPayEcoPayz, "???" },
            { Constants.PaymentSystems.ApcoPayDirect24, "DIRECTPAY" },
            { Constants.PaymentSystems.ApcoPayBoleto, "BOLETO" }
        };

		public static string CallApcoPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
		{
			using (var paymentSystemBl = new PaymentSystemBll(session, log))
			{
				using (var currencyBll = new CurrencyBll(paymentSystemBl))
				{
					var client = CacheManager.GetClientById(input.ClientId.Value);
					var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
					if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
						throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
					
					var currencyCode = currencyBll.GetCurrencyById(input.CurrencyId).Code;
					var paymentName = string.Empty;
					if (PaymentWays.ContainsKey(paymentsystem.Name))
						paymentName = PaymentWays[paymentsystem.Name];

					var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);

					var transaction = new Transaction
					{
						Hash = "hash",
						ProfileID = partnerPaymentSetting.UserName,
						ActionType = (paymentName == "MUCHBETTER" || paymentName == "CARDS") ? 4 : 1,
						Value = input.Amount,
						Curr = currencyCode,
						Email = client.Email,
						MobileNo = !string.IsNullOrEmpty(paymentInfo.CardNumber) ? paymentInfo.CardNumber : paymentInfo.MobileNumber,
						RegCountry = "NG",
						RegName = "Mr. Alan Turing",
						Lang = session.LanguageId,
						ORef = input.Id.ToString(),
						UDF1 = string.Empty,
						UDF2 = string.Empty,
						UDF3 = string.Empty,
						DOB = client.BirthDate.ToString("MM-dd-yyyy"),
						CIP = session.LoginIp,
						Address = "Street1, Street2, Marsa, MRS1231, Malta",
						ClientAcc = input.ClientId.ToString(),
						AntiFraud = new Models.ApcoPay.AntiFraudType
						{
							Provider = string.Empty
						},
						HideSSLLogo = string.Empty,
						RedirectionURL = cashierPageUrl,
						status_url = new StatusUrl
						{
							urlEncode = true,
							ENC = "UTF8",
							url = string.Format("{0}/api/ApcoPay/ApiRequest",
								  CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue)
						},
						return_pspid = string.Empty,
						FailedRedirectionURL = cashierPageUrl,
						ForcePayment = paymentName
					};
					if (paymentName == "CARDS")
					{
						//transaction.ForceBank = "RAVEDIRECTFP";
						transaction.ForceBank = "PTEST";
						transaction.RegCountry = "NG";
						transaction.RegName = null;
						transaction.NoCardList = string.Empty;
						//transaction.TEST = String.Empty;
					}
					else if (paymentName == "NEOSURF")
					{
						transaction.MainAcquirer = paymentName;
					}
					else if (!string.IsNullOrEmpty(paymentName))
					{
						transaction.FastPay = new Models.ApcoPay.FastPayType
						{
							ListAllCards = "ALL",
							NewCardOnFail = string.Empty,
							PromptCVV = string.Empty,
							PromptExpiry = string.Empty
						};
						if (paymentName == "APBT" || paymentName == "APCP")
						{
							var region = CacheManager.GetRegionById(client.RegionId, session.LanguageId);
							if (region != null && !string.IsNullOrEmpty(region.IsoCode))
								transaction.RegCountry = region.IsoCode;
							else
								transaction.RegCountry = "MX";
							if (string.IsNullOrEmpty(paymentInfo.NationalId))
								throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
							transaction.NationalID = paymentInfo.NationalId;
						}
					}
					var xmlText = CommonFunctions.ConvertToXmlWithoutNamespace(transaction);

					var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
					if (distributionUrlKey == null || distributionUrlKey.Id == 0)
						distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
					var requestUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
					var credentials = partnerPaymentSetting.Password.Split(',');
					var inputObject = new
					{
						MerchID = credentials[0],
						MerchPass = credentials[1],
						XMLParam = Uri.EscapeDataString(xmlText)
					};
					var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ApcoPayDepositUrl).StringValue;
					var httpRequestInput = new HttpRequestInput
					{
						ContentType = Constants.HttpContentTypes.ApplicationJson,
						RequestMethod = Constants.HttpRequestMethods.Post,
						Url = url,
						PostData = JsonConvert.SerializeObject(inputObject)
					};
					log.Info(xmlText);
					var responseData = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
					var output = JsonConvert.DeserializeObject<PaymentOutput>(responseData);
					if(output.Result == "OK")
						return $"{output.BaseURL}{output.Token}";
					throw new Exception(output.ErrorMsg);
				}
			}
		}

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                {
					using (var currencyBl = new CurrencyBll(partnerBl))
					{
						var client = CacheManager.GetClientById(input.ClientId.Value);
						if (client == null)
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
						var partner = CacheManager.GetPartnerById(client.PartnerId);
						if (partner == null)
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);
						var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
						if (paymentsystem == null)
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
						if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
							throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

						var url = partnerBl.GetPaymentValueByKey(partner.Id, input.PaymentSystemId, Constants.PartnerKeys.ApcoPayDepositUrl);
						string currencyCode = currencyBl.GetCurrencyById(input.CurrencyId).Code;
						var paymentName = string.Empty;
						if (PaymentWays.ContainsKey(paymentsystem.Name))
							paymentName = PaymentWays[paymentsystem.Name];

						var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
						var returnUrl = string.Format("https://{0}", partner.AdminSiteUrl.Split(',')[0]);
						var amount = input.Amount - (input.CommissionAmount ?? 0);
						var transaction = new Transaction
						{
							Hash = partnerPaymentSetting.Password,
							ProfileID = partnerPaymentSetting.UserName,
							ForcePayment = paymentName,
							ActionType = 13,
							Value = amount,
							Curr = currencyCode,
							Email = client.Email,
							//MobileNo = paymentInfo.CardNumber,
							RegCountry = "NG",
							RegName = "Mr. Alan Turing",
							Lang = session.LanguageId,
							ORef = input.Id.ToString(),
							UDF1 = string.Empty,
							UDF2 = string.Empty,
							UDF3 = string.Empty,
							DOB = client.BirthDate.ToString("yyyyMMdd"),
							CIP = session.LoginIp,
							Address = "Street1, Street2, Marsa, MRS1231, Malta",
							ClientAcc = input.ClientId.ToString(),
							AntiFraud = new Models.ApcoPay.AntiFraudType
							{
								Provider = string.Empty
							},
							HideSSLLogo = string.Empty,
							RedirectionURL = returnUrl,
							status_url = new StatusUrl
							{
								urlEncode = true,
								ENC = "UTF8",
								url = string.Format("{0}/{1}", partnerBl.GetPaymentValueByKey(client.PartnerId, null, Constants.PartnerKeys.PaymentGateway), "api/ApcoPay/ApiRequest")
							},
							return_pspid = string.Empty,
							FailedRedirectionURL = returnUrl,
							NewCard1Try = string.Empty
						};
						if (paymentName == "NEOSURF")
						{
							transaction.NeoSurfEmail = !string.IsNullOrEmpty( paymentInfo.CardNumber) ? paymentInfo.CardNumber : paymentInfo.Email;
							transaction.MainAcquirer = paymentName;
						}
						else if (paymentName == "MUCHBETTER")
						{
							var partnerDepositPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
								input.CurrencyId, (int)PaymentRequestTypes.Deposit);
							var request = paymentSystemBl.GetPaymentRequestByPaymentSetting(partnerDepositPaymentSetting.Id, (int)PaymentRequestStates.Approved);
							if (request == null)
								throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentRequestNotFound);
							transaction.PspID = request.ExternalTransactionId;
						}
						else if (!string.IsNullOrEmpty(paymentName))
						{
							transaction.FastPay = new Models.ApcoPay.FastPayType
							{
								ListAllCards = "ALL",
								NewCardOnFail = string.Empty,
								PromptCVV = string.Empty,
								PromptExpiry = string.Empty
							};
							if (paymentName == "APBT")
							{
								var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
								if (bankInfo == null)
									throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
								transaction.payoutType = "astropaytransfer";
								transaction.BeneficiaryID = paymentInfo.BeneficiaryID;
								transaction.BankName = bankInfo.BankName;
								transaction.BankBranch = bankInfo.BranchName;
								transaction.BankAccount = paymentInfo.BankAccountNumber;
								transaction.IBAN = bankInfo.IBAN;
								transaction.BankCode = bankInfo.BankCode;
								transaction.AccountType = "C";
							}
							else if (paymentName == "APCP")
							{
								transaction.payoutType = "astropaycard";
							}
                            if (string.IsNullOrEmpty(paymentInfo.NationalId))
                                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                            transaction.NationalID = paymentInfo.NationalId;
							var region = CacheManager.GetRegionById(client.RegionId, session.LanguageId);

							if (region != null && !string.IsNullOrEmpty(region.IsoCode))
								transaction.RegCountry = region.IsoCode;
							else
								transaction.RegCountry = "MX";
						}
						var xmlText = CommonFunctions.ConvertToXmlWithoutNamespace(transaction);

						XmlTextReader xmlTextRead = new XmlTextReader(new StringReader(xmlText));
						var xmlDoc = new XmlDocument();
						xmlDoc.Load(xmlTextRead);
						xmlDoc.ChildNodes[0].Attributes["hash"].Value = CommonFunctions.ComputeMd5(xmlDoc.InnerXml);

						var httpRequestInput = new HttpRequestInput
						{
							ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
							RequestMethod = Constants.HttpRequestMethods.Post,
							Url = url,
							PostData = string.Format("{0}={1}", "params", HttpUtility.UrlEncode(xmlDoc.InnerXml))
						};
						var responseData = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
						var response = new PaymentResponse
						{
							Data = responseData,
							Status = PaymentRequestStates.PayPanding,
							Description = string.Empty
						};
						return response;
					}
                }
            }
        }
    }
}
