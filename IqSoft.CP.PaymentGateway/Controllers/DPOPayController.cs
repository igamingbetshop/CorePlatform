using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.DPOPay;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using System.Xml.Serialization;
using System.IO;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class DPOPayController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??"
        };


        [HttpGet]
        [Route("api/DPOPay/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri]PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                //   BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.CompanyRef));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            if (paymentRequest.ExternalTransactionId != input.TransactionToken)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                               client.CurrencyId, paymentRequest.Type);
                            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DPOPayApiUrl).StringValue;
                            var verifyTokenInput = new VerifyInput
                            {
                                CompanyToken = partnerPaymentSetting.Password,
                                Request = "verifyToken",
                                TransactionToken = paymentRequest.ExternalTransactionId
                            };
                            var xml = SerializeAndDeserialize.SerializeToXml(verifyTokenInput, "API3G");
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationXml,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format("{0}/API/v6/", url),
                                PostData = xml
                            };
                            var deserializer = new XmlSerializer(typeof(VerifyOutput), new XmlRootAttribute("API3G"));
                            using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                            {
                                var verifyOutput = (VerifyOutput)deserializer.Deserialize(stream);
                                if (verifyOutput.Result == "000")
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                else if (verifyOutput.Result == "901" || verifyOutput.Result == "904")
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, verifyOutput.ResultExplanation, notificationBl);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            response = "OK";
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
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