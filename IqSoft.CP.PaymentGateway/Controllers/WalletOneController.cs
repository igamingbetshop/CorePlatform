using System;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.ServiceModel;
using System.Security.Cryptography;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.PaymentGateway.Models.WalletOne;
using Newtonsoft.Json;
using IqSoft.CP.Integration.Products.Helpers;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class WalletOneController : ApiController
    {
        [HttpPost]
        [Route("api/WalletOne/ApiRequest")]
        public HttpResponseMessage ApiRequest(WalletOneRequestStatus input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                    var request = paymentSystemBl.GetPaymentRequestById(input.WMI_PAYMENT_NO);
                    if (request == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    try
                    {
                        WebApiApplication.DbLogger.Info("WalletOne provider type = " + input.WMI_PAYMENT_TYPE);
                        WebApiApplication.DbLogger.Info("WMI_INVOICE_OPERATIONS = " + input.WMI_INVOICE_OPERATIONS);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.WalletOneSecretKey).StringValue;

                        var sign = input.WMI_SIGNATURE;
                        input.WMI_SIGNATURE = string.Empty;

                        Byte[] bytes = Encoding.GetEncoding("windows-1251").GetBytes(CommonFunctions.GetSortedValuesAsString(input) + secretKey);
                        Byte[] hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

                        var signature = Convert.ToBase64String(hash);

                        if (signature != sign)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                        if (input.WMI_ORDER_STATE.ToUpper() == "ACCEPTED")
                        {
                            request.ExternalTransactionId = input.WMI_ORDER_ID.ToString();
                            if (input.WMI_PAYMENT_TYPE.ToLower().Contains("creditcard"))
                            {
                                var paymentSystem = CacheManager.GetPaymentSystemByName("CreditCards");
                                request.PaymentSystemId = paymentSystem.Id;
                            }
                            paymentSystemBl.ChangePaymentRequestDetails(request);

                            response = "WMI_RESULT=OK";
							clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                            BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            return new HttpResponseMessage { Content = new StringContent(response, Encoding.UTF8) };
                        }
                        else
                        {
                            response = "WMI_RESULT=RETRY";
                            return new HttpResponseMessage { Content = new StringContent(response, Encoding.UTF8) };
                        }
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            response = "WMI_RESULT=OK";
                            return new HttpResponseMessage { Content = new StringContent(response, Encoding.UTF8) };
                        }
                        WebApiApplication.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                        response = "WMI_RESULT=RETRY&WMI_DESCRIPTION=" + ex.Message;
                        return new HttpResponseMessage { Content = new StringContent(response, Encoding.UTF8) };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                        response = "WMI_RESULT=RETRY&WMI_DESCRIPTION=" + ex.Message;
                        return new HttpResponseMessage { Content = new StringContent(response, Encoding.UTF8) };
                    }
                }
            }
        }
    }
}