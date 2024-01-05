using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.ApcoPay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class ApcoPayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "213.165.190.20",
            "217.168.166.66"
        };

        [HttpGet]
        [HttpPost]
        [Route("api/ApcoPay/ApiRequest")]
        public ActionResult ApiRequest()
        {
            var response = new PaymentResultOutput
            {
                Transaction = new TransactionOutput()
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var bodyStream = new StreamReader(Request.Body);
                var inputParams = HttpUtility.UrlDecode(bodyStream.ReadToEnd()).Replace("params=", string.Empty);
                Program.DbLogger.Info("input: " + inputParams);
                var deserializer = new XmlSerializer(typeof(Transaction));
                using var reader = new StringReader(inputParams);
                var inputResult = (Transaction)deserializer.Deserialize(reader);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(clientBl);

                var request = paymentSystemBl.GetPaymentRequestById(inputResult.ORef);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                response.Transaction.ORef = request.Id.ToString();
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                var startIndex = inputParams.IndexOf("hash=\"") + 6;
                var endIndex = inputParams.IndexOf("\">", startIndex);
                var hash = inputParams.Substring(startIndex, endIndex - startIndex);
                var inputHash = inputResult.Hash;
                inputParams = inputParams.Replace(hash, partnerPaymentSetting.Password);
                hash = CommonFunctions.ComputeMd5(inputParams);
                if (hash.ToLower() != inputHash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                if (inputResult.Result.ToUpper() == "OK")
                {
                    request.ExternalTransactionId = inputResult.pspid;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                    {
                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                        response.Transaction.Result = ApcoHelpers.Statuses.Success;
                    }
                    else
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty, request.CashDeskId, 
                                                                       null, true, request.Parameters, documentBl, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                    }
                }
                else
                    response.Transaction.Result = ApcoHelpers.Statuses.Failed;
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
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
            }
            catch (Exception ex)
            {
                response.Transaction.Result = ApcoHelpers.Statuses.Failed;
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml,
                                                         CustomXmlFormatter.XmlFormatterTypes.Utf8Format));
        }
    }
}