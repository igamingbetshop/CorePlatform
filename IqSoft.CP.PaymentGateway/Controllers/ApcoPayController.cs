using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.ApcoPay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class ApcoPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.ApcoPay);

        [HttpGet]
        [HttpPost]
        [Route("api/ApcoPay/ApiRequest")]
        public HttpResponseMessage ApiRequest()
        {
            var response = new PaymentResultOutput
            {
                Transaction = new TransactionOutput()
            };
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var userIds = new List<int>();
                var inputParams = HttpUtility.UrlDecode(bodyStream.ReadToEnd()).Replace("params=", string.Empty);
                WebApiApplication.DbLogger.Info("input: " + inputParams);
                var deserializer = new XmlSerializer(typeof(Transaction));
                using (var reader = new StringReader(inputParams))
                {
                    var inputResult = (Transaction)deserializer.Deserialize(reader);
                    using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        {
                            using (var documentBl = new DocumentBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(clientBl))
                                {
                                    BaseBll.CheckIp(WhitelistedIps);
                                    var request = paymentSystemBl.GetPaymentRequestById(inputResult.ORef);
                                    if (request == null)
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                    response.Transaction.ORef = request.Id.ToString();
                                    var client = CacheManager.GetClientById(request.ClientId.Value);
                                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                                    var inputHash = inputResult.Hash;
                                    var sw = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ApcoPaySecretWord).StringValue;

                                    inputParams = inputParams.Replace(inputHash, sw);
                                    var hash = CommonFunctions.ComputeMd5(inputParams);
                                    WebApiApplication.DbLogger.Info("InputParams: " + inputParams + ", " + "CalculatedHash: " + hash);
                                    if (hash.ToLower() != inputHash.ToLower())
                                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                                    if (inputResult.Result.ToUpper() == "OK")
                                    {
                                        request.ExternalTransactionId = inputResult.pspid;
                                        paymentSystemBl.ChangePaymentRequestDetails(request);
                                        if (request.Type == (int)PaymentRequestTypes.Deposit)
                                        {
                                            clientBl.ApproveDepositFromPaymentSystem(request, false, out userIds);
                                            response.Transaction.Result = ApcoHelpers.Statuses.Success;
                                        }
                                        else
                                        {
                                            var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, 
                                                string.Empty, request.CashDeskId, null, true, request.Parameters, documentBl, notificationBl, out userIds);

                                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                        }
                                        foreach (var uId in userIds)
                                        {
                                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                        }
                                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                    }
                                    else
                                        response.Transaction.Result = ApcoHelpers.Statuses.Failed;
                                }
                            }
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
                    response.Transaction.Result = ApcoHelpers.Statuses.Success;
                }
                else response.Transaction.Result = ApcoHelpers.Statuses.Failed;
                WebApiApplication.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
            }
            catch (Exception ex)
            {
                response.Transaction.Result = ApcoHelpers.Statuses.Failed;
                WebApiApplication.DbLogger.Error(ex);
            }
            return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml, CustomXmlFormatter.XmlFormatterTypes.Utf8Format);
        }
    }
}