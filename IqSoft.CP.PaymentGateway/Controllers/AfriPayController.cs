using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using System.Web.Http;
using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using System.Net;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Net.Http.Headers;
using IqSoft.CP.PaymentGateway.Models.AfriPay;
using System.IO;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class AfriPayController : ApiController
    {
        private static readonly string CertificatePath = @"C:\Certificates\{0}\client_{1}.p12";

        [HttpPost]
        [Route("api/AfriPay/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId))??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        paymentRequest.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    if (paymentRequest.Status != (int)PaymentRequestStates.Pending)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.AfriPayApiUrl);
                    var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                    var merchant = partnerPaymentSetting.UserName.Split(',');
                    var point = merchant[0];
                    var service = merchant[1];
                    var ind = input.CardNumber.Length - 5;
                    var panMask = $"{input.CardNumber.Substring(0, 5)}*****{input.CardNumber.Substring(ind+1, input.CardNumber.Length - ind -1)}";
                    var date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+0000");
                    var processPaymentInput = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                              $" <request point=\"{point}\">" +
                                              $" <opayment  id=\"{paymentRequest.Id}\"" +
                                              $"            sum=\"{(int)paymentRequest.Amount*100}\"" +
                                              $"            check=\"0\"" +
                                              $"            service=\"{service}\"" +
                                              $"            account=\"{panMask}\" " +
                                              $"            date=\"{date}\">" +
                                              $"     <attribute name=\"amount_currency\" value=\"{client.CurrencyId}\"/>" +
                                              $"     <attribute name=\"redirect_method\" value=\"GET\"/>\r\n   " +
                                              $"     <attribute name=\"redirect_url\" value=\"{input.RedirectUrl}\"/>\r\n   " +
                                              $"     <attribute name=\"notify_url\" value=\"{string.Format("{0}/api/AfriPay/ApiRequest", paymentGateway)}\"/>\r\n      " +
                                              $"     <attribute name=\"card_pan\" value=\"{input.CardNumber}\"/>\r\n     " +
                                              $"     <attribute name=\"card_name\" value=\"{input.HolderName}\"/>\r\n    " +
                                              $"     <attribute name=\"card_cvv\" value=\"{input.VerificationCode}\"/>\r\n       " +
                                              $"     <attribute name=\"card_year\" value=\"{input.ExpiryYear.Substring(2, 2)}\"/>\r\n" +
                                              $"     <attribute name=\"card_month\" value=\"{input.ExpiryMonth}\"/>\r\n " +
                                              $" </opayment> </request>";
                    var certificate = new X509Certificate2(string.Format(CertificatePath, Constants.PaymentSystems.AfriPay.ToLower(), client.PartnerId),
                                                                         partnerPaymentSetting.Password, X509KeyStorageFlags.MachineKeySet);

                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationXml,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = processPaymentInput
                    };

                    try
                    {
                        var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate);
                        WebApiApplication.DbLogger.Info("Resp: " +  res);
                        var serializer = new XmlSerializer(typeof(PaymentResponse), new XmlRootAttribute("response"));
                        var paymentResponse = (PaymentResponse)serializer.Deserialize(new StringReader(res));
                        if (paymentResponse.result != null)
                        {
                            paymentRequest.ExternalTransactionId = paymentResponse.result.trans;
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (paymentResponse.result.state == 80)
                            {
                                var comment = $"Status: {paymentResponse.result.state}";
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, comment, notificationBl);
                                throw new Exception(comment);
                            }
                            else if (!paymentResponse.result.final)
                            {
                                result.RedirectUrl = paymentResponse.result.attribute.FirstOrDefault(x => x.name == "auth.url")?.value;
                                if (string.IsNullOrEmpty(result.RedirectUrl))
                                {
                                    var htmlForm = paymentResponse.result.attribute.FirstOrDefault(x => x.name == "auth.redirect.html")?.value;
                                    if (!string.IsNullOrEmpty(htmlForm))
                                    {
                                        var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                                        if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                                            distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
                                        var requestParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                                        var distributionUrl = string.Format(distributionUrlKey.StringValue, requestParameters["Domain"]);
                                        htmlForm = htmlForm.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"");
                                        result.RedirectUrl = string.Format("{0}/paymentform/paymentform?htmlForm={1}", distributionUrl, htmlForm);
                                    }
                                }
                            }
                        }
                        else
                            throw new Exception(res);
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                        using (var clientBll = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                        {
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, ex.Message, notificationBl);
                            throw;
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

        [HttpGet]
        [Route("api/AfriPay/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri] PaymentInput input)
        {
            var response = "SUCCESS";
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " +  JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.refNo)) ??
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                if (input.Status == 60) 
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, input.Descriptor);
                                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                }
                                else if(input.Status == 40)
                                {
                                    var comment = $"Status: {input.Status}, Message: {input.Descriptor}";
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, comment, notificationBl);
                                }
                            }
                            else
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(new Exception(response));
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}