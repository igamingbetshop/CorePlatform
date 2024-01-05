using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.ProductGateway.Models.SmartSoft;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using System;
using System.Net;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using System.IO;

using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class SmartSoftController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SmartSoft).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "213.227.140.30",
            "83.149.106.250",
            "81.171.20.92"
        };
        //public static List<string> UnsuppordedCurrenies = new List<string>
        //{
        //    Currencies.IranianRial
        //};

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/activateSession")]
        public ActionResult CheckSession(ActivateSessionInput input)
        {
            var signature = Request.Headers["X-Signature"].ToString();
            try
            {
                Program.DbLogger.Info("CheckSession_" + JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var bodyStream = new StreamReader(Request.Body);
                    var inputString = bodyStream.ReadToEnd();
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftKey);
                    var portalName = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftPortalName);
                    var stringToSign = key + "|POST|" + inputString;
                    var sign = CommonFunctions.ComputeMd5(stringToSign).ToLower();
                    if (signature.ToLower() != sign)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                    var response = new
                    {
                        SessionId = clientSession.Token,
                        client.UserName,
                        ClientExternalKey = client.Id.ToString(),
                        PortalName = portalName,
                        CurrencyCode =/* UnsuppordedCurrenies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar :*/ client.CurrencyId
                    };
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + signature + "_" + fex.Detail.Message);
                Response.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                Response.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return BadRequest(JsonConvert.SerializeObject(fex.Detail.Message));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                Response.Headers.Add("X_ErrorMessage", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{partnerId}/api/smartsoft/getbalance")]
        public ActionResult GetBalance()
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var sessionId = Request.Headers["X-SessionId"];
                var username = Request.Headers["X-UserName"];
                var clientKey = Request.Headers["X-ClientExternalKey"];
                var signature = Request.Headers["X-Signature"].ToString();
                var clientSession = ClientBll.GetClientProductSession(sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.UserName != username || client.Id.ToString() != clientKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftKey);
                var sign = CommonFunctions.ComputeMd5(key + "|GET|").ToLower();
                if (signature.ToLower() != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                var currency = client.CurrencyId;
                //if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                //{
                //    currency = Constants.Currencies.USADollar;
                //    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                //}
                var response = new
                {
                    Amount = string.Format("{0:N2}", balance),
                    CurrencyCode = currency
                };
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail.Message);
                Response.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                Response.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return BadRequest(JsonConvert.SerializeObject(fex.Detail.Message));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                Response.Headers.Add("X_ErrorMessage", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/deposit")]
        public ActionResult DoBet(TransactionInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);

                var bodyStream = new StreamReader(Request.Body);
                var inputString = bodyStream.ReadToEnd();
                var sessionId = Request.Headers["X-SessionId"].ToString();
                var username =Request.Headers["X-UserName"];
                var clientKey = Request.Headers["X-ClientExternalKey"];
                var signature = Request.Headers["X-Signature"].ToString();
                var clientSession = ClientBll.GetClientProductSession(sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.UserName != username || client.Id.ToString() != clientKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftKey);
                var sign = CommonFunctions.ComputeMd5(key + "|POST|" + inputString).ToLower();
                if (signature.ToLower() != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
               
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                long transctionId = 0;
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id,
                                       ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        var currency = client.CurrencyId;
                        //if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                        //    currency = Constants.Currencies.USADollar;

                        if (document == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                RoundId = input.TransactionInfo.RoundId,
                                TransactionId = input.TransactionId.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = BaseBll.ConvertCurrency(currency, client.CurrencyId, input.Amount),
                                DeviceTypeId = clientSession.DeviceType
                            });
                            transctionId = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl).Id;
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }

                        var response = new
                        {
                            TransactionId = transctionId.ToString(),
                            Balance = string.Format("{0:N2}", BaseBll.ConvertCurrency(client.CurrencyId, currency,
                            BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                            CurrencyCode = client.CurrencyId
                        };
                        return Ok(response);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail.Message);
                Response.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                Response.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return BadRequest(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                Response.Headers.Add("X_ErrorMessage", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/withdraw")]
        public ActionResult DoWin(TransactionInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var bodyStream = new StreamReader(Request.Body);
                var inputString = bodyStream.ReadToEnd();
                var sessionId = Request.Headers["X-SessionId"];
                var username = Request.Headers["X-UserName"];
                var clientKey = Request.Headers["X-ClientExternalKey"];
                var signature = Request.Headers["X-Signature"].ToString();
                var clientSession = ClientBll.GetClientProductSession(sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.UserName != username || client.Id.ToString() != clientKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftKey);
                var sign = CommonFunctions.ComputeMd5(key + "|POST|" + inputString).ToLower();
                if (signature.ToLower() != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);


                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                long transctionId = 0;
                var currency = client.CurrencyId;
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        if (!string.IsNullOrEmpty(input.TransactionType) && input.TransactionType.ToLower() == "closeround")
                        {
                            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.TransactionInfo.RoundId, ProviderId,
                                                                                                   client.Id, (int)BetDocumentStates.Uncalculated);
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                State = (int)BetDocumentStates.Lost,
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.TransactionInfo.RoundId,
                                GameProviderId = ProviderId,
                                ProductId = clientSession.ProductId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            foreach (var betDoc in betDocuments)
                            {
                                betDoc.State = (int)BetDocumentStates.Lost;
                                listOfOperationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                                listOfOperationsFromApi.CreditTransactionId = betDoc.Id;
                                transctionId = clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl)[0].Id;
                            }
                        }
                        else
                        {
                            var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.TransactionInfo.RoundId,
                                                ProviderId, client.Id);
                            if (betDocument == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                            var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                            //if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                            //    currency = Constants.Currencies.USADollar;
                            if (winDocument == null)
                            {
                                var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                                betDocument.State = state;
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    CurrencyId = client.CurrencyId,
                                    RoundId = input.TransactionInfo.RoundId,
                                    GameProviderId = ProviderId,
                                    OperationTypeId = (int)OperationTypes.Win,
                                    ExternalOperationId = null,
                                    ExternalProductId = product.ExternalId,
                                    ProductId = betDocument.ProductId,
                                    TransactionId = input.TransactionId.ToString(),
                                    CreditTransactionId = betDocument.Id,
                                    State = state,
                                    Info = string.Empty,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                var amount = BaseBll.ConvertCurrency(currency, client.CurrencyId, input.Amount);
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = amount
                                });

                                transctionId = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0].Id;
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    Amount = amount,
                                    CurrencyId = currency,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                        }
                        var response = new
                        {
                            TransactionId = transctionId.ToString(),
                            Balance = string.Format("{0:N2}", BaseBll.ConvertCurrency(client.CurrencyId, currency,
                            BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                            CurrencyCode = client.CurrencyId
                        };
                        return Ok(response);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail.Message);
                Response.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                Response.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return BadRequest(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                Response.Headers.Add("X_ErrorMessage", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/rollback")]
        public ActionResult Rollback(TransactionInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var inputString = Request.QueryString.ToString();
                var sessionId = Request.Headers["X-SessionId"];
                var username = Request.Headers["X-UserName"];
                var clientKey = Request.Headers["X-ClientExternalKey"];
                var signature = Request.Headers["X-Signature"];
                var clientSession = ClientBll.GetClientProductSession(sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.UserName != username || client.Id.ToString() != clientKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftKey);

                var sign = CommonFunctions.ComputeMd5(key + "|POST|" + inputString).ToLower();
                if (signature.ToString().ToLower() != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);


                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.TransactionId.ToString(),
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId
                    };

                    var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                    if (documents == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    var currency = client.CurrencyId;
                    //if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    //    currency = Constants.Currencies.USADollar;
                    var response = new
                    {
                        TransactionId = documents[0].Id.ToString(),
                        Balance = string.Format("{0:N2}", BaseBll.ConvertCurrency(client.CurrencyId, currency,
                                  BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                        CurrencyCode = client.CurrencyId
                    };
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail.Message);
                Response.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                Response.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return BadRequest(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                Response.Headers.Add("X_ErrorMessage", ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}