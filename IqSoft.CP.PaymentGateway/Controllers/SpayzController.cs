using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Spayz;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class SpayzController : ApiController
    {
        [HttpPost]
        [Route("api/Spayz/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput paymentInput)
        {
            var response = new PaymentOutput();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                var inputSign = HttpContext.Current.Request.Headers.Get("Authorization");
                WebApiApplication.DbLogger.Info("inputSign: " + inputSign);
                if (string.IsNullOrEmpty(inputSign))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash, info: inputSign);
                var signValues = inputSign.Split(' ');
                if (signValues.Length != 2)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash, info: inputSign);
                signValues = signValues[1].Split(':');
                if (signValues.Length != 2)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash, info: inputSign);               
                var hash = signValues[1];
                var generatedSign = GetSignatureString(paymentInput);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(paymentInput.Order.Moid)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var pubKey = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.SpayzPublicKey);
                    if (VerifySignature(pubKey, generatedSign, hash))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash, info: inputSign);

                    if (paymentInput.Order.ActualAmount.Currency != paymentRequest.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    if (paymentInput.Order.ActualAmount.PValue/100 != paymentRequest.Amount)
                    {
                        var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                         JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);

                        if (!parameters.ContainsKey("ActualAmount"))
                            parameters.Add("ActualAmount", (paymentInput.Order.ActualAmount.PValue/100).ToString());
                        else
                            parameters["ActualAmount"] = (paymentInput.Order.ActualAmount.PValue/100).ToString();
                        if (!parameters.ContainsKey("InitialAmount"))
                            parameters.Add("InitialAmount", paymentRequest.Amount.ToString());
                        else
                            parameters["InitialAmount"] = paymentRequest.Amount.ToString();
                        paymentRequest.Amount = paymentInput.Order.ActualAmount.PValue / 100;
                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    }
                    var status = paymentInput.Order.Status.ToLower();
                    if (status == "done")
                    {
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                        foreach (var uId in userIds)
                        {
                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    else if (status == "rejected" || status == "refunded")
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, status, notificationBl);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                    response.Status = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;

                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {bodyStream.ReadToEnd()}" +
                                                 $" Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = ex.Message;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
        }

        static bool VerifySignature(string publicKey, string data, string signatureHex)
        {
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKey);

                byte[] signature = Convert.FromBase64String(signatureHex);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                return rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }
        }

        private static string GetSignatureString(PaymentInput input)
        {
            var sortedList = GetFieldNamesWithValues(input);
            var result = sortedList.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + par.Value + "&");
            return result.Remove(result.LastIndexOf("&"), 1);
        }
        public static SortedDictionary<string, object> GetFieldNamesWithValues(object input, string prefix = "")
        {
            var sortedParams = new SortedDictionary<string, object>();
            if (input == null) return sortedParams;

            Type type = input.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance |
                       BindingFlags.Static |
                       BindingFlags.NonPublic |
                       BindingFlags.Public);
            IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(type).OfType<PropertyDescriptor>();

            foreach (var field in properties)
            {
                object value = field.GetValue(input);
                var jsonName = field.Attributes.OfType<JsonPropertyAttribute>().FirstOrDefault()?.PropertyName ?? field.Name;
                string fieldName = string.IsNullOrEmpty(prefix) ? jsonName : $"{prefix}.{jsonName}";
                if (value != null)
                {
                    var fieldType = value.GetType();
                    if (fieldType.IsPrimitive || fieldType == typeof(string) || fieldType == typeof(decimal))
                        sortedParams.Add(fieldName, value);
                    else
                    {
                        var res = GetFieldNamesWithValues(value, fieldName);
                        foreach (var r in res)
                            sortedParams.Add(r.Key, r.Value);
                    }
                }
            }
            return sortedParams;
        }
    }
}