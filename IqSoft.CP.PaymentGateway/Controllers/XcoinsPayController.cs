using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System;
using Newtonsoft.Json;
using IqSoft.CP.PaymentGateway.Models.XcoinsPay;
using System.Collections.Generic;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class XcoinsPayController : ApiController
    {
        private static readonly List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.XcoinsPayCrypto);
        [HttpPost]
        [Route("api/XcoinsPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = new { Status = 0, Description = "Ok" };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);

                var inputSign = HttpContext.Current.Request.Headers.Get("Xcoins-Signature");
                if (string.IsNullOrEmpty(inputSign))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var inputNonce = HttpContext.Current.Request.Headers.Get("Nonce");
                if (string.IsNullOrEmpty(inputNonce))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Reference)) ?? 
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(request.ClientId.Value);
                    var publicKey = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, request.PaymentSystemId, Constants.PartnerKeys.XcoinsPayApiPublicKey);
                    if (!ValidateSignature(inputSign, inputNonce, publicKey, inputString))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                       client.CurrencyId, request.Type);
                    var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters) ?? new Dictionary<string, string>();
                    if (parameters.ContainsKey("FromCurrencyCode"))
                        parameters["FromCurrencyCode"] = input.FromCurrency.Code;
                    else
                        parameters.Add("FromCurrencyCode", input.FromCurrency.Code);
                    if (parameters.ContainsKey("ToCurrencyCode"))
                        parameters["ToCurrencyCode"] = input.ToCurrency.Code;
                    else
                        parameters.Add("ToCurrencyCode", input.ToCurrency.Code);

                    if (parameters.ContainsKey("FromAmount"))
                        parameters["FromAmount"] = input.AmountFrom.ToString();
                    else
                        parameters.Add("FromAmount", input.AmountFrom.ToString());
                    if (parameters.ContainsKey("ToAmount"))
                        parameters["ToAmount"] = input.AmountTo.ToString();
                    else
                        parameters.Add("ToAmount", input.AmountTo.ToString());
                    request.Amount =Convert.ToDecimal(input.AmountTo);
                    request.Parameters = JsonConvert.SerializeObject(parameters);
                    request.ExternalTransactionId = input.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(request);

                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            using (var documentBll = new DocumentBll(paymentSystemBl))
                            {
                                if (input.Status.ToUpper() == "ACCEPTED" || input.Status.ToUpper() == "COMPLETED")
                                {
                                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                                    else
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                       null, null, false, request.Parameters, documentBll, notificationBl);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                                else if (input.Status.ToUpper() == "REJECTED" || input.Status.ToUpper() == "FAILED")
                                {
                                    var reason = $"Status: {input.Status}";
                                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, reason, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, reason,
                                                                            null, null, false, string.Empty, documentBll, notificationBl);   
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                        }
                    }
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new { Status = "OK" }), Encoding.UTF8)
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {bodyStream.ReadToEnd()}" +
                                                $" Response: {JsonConvert.SerializeObject(response)}");
                if (ex.Detail?.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent(JsonConvert.SerializeObject(new { Status = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName }), Encoding.UTF8)
                    };
                else
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(new { Status = "OK" }), Encoding.UTF8)
                    };
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(JsonConvert.SerializeObject(new { Status = ex }), Encoding.UTF8)
                };                
            }            
        }

        public static RSACryptoServiceProvider ImportPublicKey(string pemBody)
        {
            PemReader pr = new PemReader(new StringReader(pemBody));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsaParams);
            return csp;
        }

        public bool ValidateSignature(string signature, string nonce, string publicKey, string body)
        {
            using (var rsa = ImportPublicKey(publicKey))
            using (SHA512Managed sha512 = new SHA512Managed())
            {
                byte[] data = Encoding.UTF8.GetBytes(body + nonce);
                byte[] signatureBytes = Convert.FromBase64String(signature);

                return rsa.VerifyData(data, sha512, signatureBytes);
            }
        }
    }
}