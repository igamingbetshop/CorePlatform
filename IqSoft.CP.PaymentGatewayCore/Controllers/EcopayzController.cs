using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.Ecopayz;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;
using Voucher = IqSoft.CP.PaymentGateway.Models.Ecopayz.Voucher;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class EcopayzController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "176.57.42.131",
            "176.57.42.132",
            "176.57.40.247",
            "176.57.40.250"
        };

        [HttpPost]
        [Route("api/EcoPayz/Notify")]
        public ActionResult NotifyRequest(HttpRequestMessage input)
        {
            var merchantPass = string.Empty;
            var xmlData = string.Empty;
            var response = new SVSPurchaseStatusNotificationResponse
            {
                TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                {
                    Code = 0,
                    Description = "Success"
                },
                Status = "Confirmed"
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var inputString = input.Content.ReadAsStringAsync();
                Program.DbLogger.Info("inputString:" + inputString.Result);

                var serializer = new XmlSerializer(typeof(SVSPurchaseStatusNotificationRequest), new XmlRootAttribute("SVSPurchaseStatusNotificationRequest"));
                var inputObject = (SVSPurchaseStatusNotificationRequest)serializer.Deserialize(new StringReader(inputString.Result.Replace("XML=",string.Empty)));
                var checksum = inputObject.Authentication.Checksum;
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var request = paymentSystemBl.GetPaymentRequestById(inputObject.Request.TxID);
                        if (request == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                             request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        merchantPass = partnerPaymentSetting.Password;
                        xmlData = inputString.Result.Replace("XML=",string.Empty).Replace(checksum, partnerPaymentSetting.Password);
                        if (checksum != CommonFunctions.ComputeMd5(xmlData))
                           throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        request.ExternalTransactionId = inputObject.StatusReport.SVSTransaction.BatchNumber;
                        paymentSystemBl.ChangePaymentRequestDetails(request);                       
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));
              
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = new SVSPurchaseStatusNotificationResponse
                {
                    TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                    {
                        Code = 10020,
                        Description = exp.Message
                    },
                    Status = "Failed"
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new SVSPurchaseStatusNotificationResponse
                {
                    TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                    {
                        Code = 10020,
                        Description = ex.Message
                    },
                    Status = "Failed"
                };
            }
            response.Authentication = new SVSPurchaseStatusNotificationResponseAuthentication
            {
                Checksum = merchantPass
            };
            xmlData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<SVSPurchaseStatusNotificationResponse>(response)
                .Replace("\r\n", string.Empty).Replace(" ", string.Empty); 
            response.Authentication.Checksum = CommonFunctions.ComputeMd5(xmlData);
            xmlData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<SVSPurchaseStatusNotificationResponse>(response)
                .Replace("\r\n", string.Empty).Replace(" ", string.Empty);
            Program.DbLogger.Info("OutputString:" + xmlData);
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(new StringContent(xmlData, Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/EcoPayz/PayVoucher")]
        public ActionResult PayVoucher(HttpRequestMessage input)
        {
            var merchantPass = string.Empty;
            var xmlData = string.Empty;
            var response = new SVSPurchaseStatusNotificationResponse
            {
                TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                {
                    Code = 0,
                    Description = "Success"
                },
                Status = "Confirmed"
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var inputString = input.Content.ReadAsStringAsync();
                Program.DbLogger.Info("inputString:" + inputString.Result);

                var serializer = new XmlSerializer(typeof(Voucher.SVSPurchaseStatusNotificationRequest), new XmlRootAttribute("SVSPurchaseStatusNotificationRequest"));
                var inputObject = (Voucher.SVSPurchaseStatusNotificationRequest)serializer.Deserialize(new StringReader(inputString.Result.Replace("XML=", string.Empty)));
                var checksum = inputObject.Authentication.Checksum;
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(inputObject.Request.TxID);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                     request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                merchantPass = partnerPaymentSetting.Password;
                xmlData = inputString.Result.Replace("XML=", string.Empty).Replace(checksum, partnerPaymentSetting.Password);
                if (checksum != CommonFunctions.ComputeMd5(xmlData))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                request.ExternalTransactionId = inputObject.StatusReport.SVSTransaction.BatchNumber;
                var amount = inputObject.Request.Amount;
                if (inputObject.Request.Currency != client.CurrencyId)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(partnerPaymentSetting.CurrencyId, client.CurrencyId, partnerPaymentSetting);
                    amount *= rate;
                    var parameters = string.IsNullOrEmpty(request.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                    parameters.Add("Currency", Constants.Currencies.USADollar);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    request.Parameters = JsonConvert.SerializeObject(parameters);
                }
                request.Amount = Math.Floor(amount * 100) / 100;
                paymentSystemBl.ChangePaymentRequestDetails(request);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))

                    return Ok(new StringContent("OK", Encoding.UTF8));

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = new SVSPurchaseStatusNotificationResponse
                {
                    TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                    {
                        Code = 10020,
                        Description = exp.Message
                    },
                    Status = "Failed"
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new SVSPurchaseStatusNotificationResponse
                {
                    TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                    {
                        Code = 10020,
                        Description = ex.Message
                    },
                    Status = "Failed"
                };
            }
            response.Authentication = new SVSPurchaseStatusNotificationResponseAuthentication
            {
                Checksum = merchantPass
            };
            xmlData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<SVSPurchaseStatusNotificationResponse>(response)
                .Replace("\r\n", string.Empty).Replace(" ", string.Empty);
            response.Authentication.Checksum = CommonFunctions.ComputeMd5(xmlData);
            xmlData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<SVSPurchaseStatusNotificationResponse>(response)
                .Replace("\r\n", string.Empty).Replace(" ", string.Empty);
            Program.DbLogger.Info("OutputString:" + xmlData);
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(new StringContent(xmlData, Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/EcoPayz/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage input)
        {
            var merchantPass = string.Empty;
            var xmlData = string.Empty;
            var response = new SVSPurchaseStatusNotificationResponse
            {
                TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                {
                    Code = 0,
                    Description = "Success"
                },
                Status = "Confirmed"
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var inputString = input.Content.ReadAsStringAsync();
                Program.DbLogger.Info("inputString:" + inputString.Result);

                var serializer = new XmlSerializer(typeof(TransactionResult));
                var inputObject = (TransactionResult)serializer.Deserialize(new StringReader(inputString.Result.Replace("XML=", string.Empty)));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(inputObject.ClientTransactionID);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                if (request.Status != (int)PaymentRequestStates.Pending)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                if (inputObject.ErrorCode == 0) // ??
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                else
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, inputObject.Message, notificationBl);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = new SVSPurchaseStatusNotificationResponse
                {
                    TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                    {
                        Code = 10020,
                        Description = exp.Message
                    },
                    Status = "Failed"
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new SVSPurchaseStatusNotificationResponse
                {
                    TransactionResult = new SVSPurchaseStatusNotificationResponseTransactionResult
                    {
                        Code = 10020,
                        Description = ex.Message
                    },
                    Status = "Failed"
                };
            }
            response.Authentication = new SVSPurchaseStatusNotificationResponseAuthentication
            {
                Checksum = merchantPass
            };
            xmlData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<SVSPurchaseStatusNotificationResponse>(response)
                .Replace("\r\n", string.Empty).Replace(" ", string.Empty);
            response.Authentication.Checksum = CommonFunctions.ComputeMd5(xmlData);
            xmlData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<SVSPurchaseStatusNotificationResponse>(response)
                .Replace("\r\n", string.Empty).Replace(" ", string.Empty);
            Program.DbLogger.Info("OutputString:" + xmlData);
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(new StringContent(xmlData, Encoding.UTF8));
        }
    }
}