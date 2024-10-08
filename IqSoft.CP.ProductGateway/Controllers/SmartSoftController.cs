using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.ProductGateway.Models.SmartSoft;
using System.Web.Http;
using System.Web;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using System;
using System.Net;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using System.Web.Http.Cors;
using System.IO;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SmartSoftController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SmartSoft).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.SmartSoft);

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/activateSession")]
        public HttpResponseMessage CheckSession(ActivateSessionInput input)
        {
            var signature = HttpContext.Current.Request.Headers.Get("X-Signature");
            try
            {
                WebApiApplication.DbLogger.Info("CheckSession_" + JsonConvert.SerializeObject(input));
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
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
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(response))
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + signature + "_" + fex.Detail.Message);
                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(JsonConvert.SerializeObject(fex.Detail.Message))
                };
                res.Content.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                res.Content.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return res;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);

                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(ex.Message)
                };
                res.Content.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                res.Content.Headers.Add("X_ErrorMessage", ex.Message);
                return res;
            }
        }

        [HttpGet]
        [Route("{partnerId}/api/smartsoft/getbalance")]
        public HttpResponseMessage GetBalance([FromUri]int partnerId)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var sessionId = HttpContext.Current.Request.Headers.Get("X-SessionId");
                var username = HttpContext.Current.Request.Headers.Get("X-UserName");
                var clientKey = HttpContext.Current.Request.Headers.Get("X-ClientExternalKey");
                var signature = HttpContext.Current.Request.Headers.Get("X-Signature");
                var clientSession = ClientBll.GetClientProductSession(sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.UserName != username || client.Id.ToString() != clientKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SmartSoftKey);
                var sign = CommonFunctions.ComputeMd5(key + "|GET|").ToLower();
                if (signature.ToLower() != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
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
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(response))
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(JsonConvert.SerializeObject(fex.Detail.Message))
                };
                res.Content.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                res.Content.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return res;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);

                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(ex.Message)
                };
                res.Content.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                res.Content.Headers.Add("X_ErrorMessage", ex.Message);
                return res;
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/deposit")]
        public HttpResponseMessage DoBet(int partnerId, TransactionInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);

                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                var sessionId = HttpContext.Current.Request.Headers.Get("X-SessionId");
                var username = HttpContext.Current.Request.Headers.Get("X-UserName");
                var clientKey = HttpContext.Current.Request.Headers.Get("X-ClientExternalKey");
                var signature = HttpContext.Current.Request.Headers.Get("X-Signature");
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
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                            transctionId = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info).Id;
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }

                        var response = new
                        {
                            TransactionId = transctionId.ToString(),
                            Balance = string.Format("{0:N2}", BaseBll.ConvertCurrency(client.CurrencyId, currency,
                            BaseHelpers.GetClientProductBalance(client.Id, product.Id))),
                            CurrencyCode = client.CurrencyId
                        };
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonConvert.SerializeObject(response))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(JsonConvert.SerializeObject(fex.Detail.Message))
                };
                res.Content.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                res.Content.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return res;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);

                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(ex.Message)
                };
                res.Content.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                res.Content.Headers.Add("X_ErrorMessage", ex.Message);
                return res;
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/withdraw")]
        public HttpResponseMessage DoWin(TransactionInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                var sessionId = HttpContext.Current.Request.Headers.Get("X-SessionId");
                var username = HttpContext.Current.Request.Headers.Get("X-UserName");
                var clientKey = HttpContext.Current.Request.Headers.Get("X-ClientExternalKey");
                var signature = HttpContext.Current.Request.Headers.Get("X-Signature");
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
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                                    BetId = betDocument?.Id ?? 0,
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    BetAmount = betDocument?.Amount,
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
                            BaseHelpers.GetClientProductBalance(client.Id, product.Id))),
                            CurrencyCode = client.CurrencyId
                        };
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonConvert.SerializeObject(response))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(JsonConvert.SerializeObject(fex.Detail.Message))
                };
                res.Content.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                res.Content.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return res;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);

                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(ex.Message)
                };
                res.Content.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                res.Content.Headers.Add("X_ErrorMessage", ex.Message);
                return res;
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/smartsoft/rollback")]
        public HttpResponseMessage Rollback(int partnerId, TransactionInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var inputString = HttpContext.Current.Request.QueryString.ToString();
                var sessionId = HttpContext.Current.Request.Headers.Get("X-SessionId");
                var username = HttpContext.Current.Request.Headers.Get("X-UserName");
                var clientKey = HttpContext.Current.Request.Headers.Get("X-ClientExternalKey");
                var signature = HttpContext.Current.Request.Headers.Get("X-Signature");
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
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                    var currency = client.CurrencyId;
                    //if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    //    currency = Constants.Currencies.USADollar;
                    var response = new
                    {
                        TransactionId = documents[0].Id.ToString(),
                        Balance = string.Format("{0:N2}", BaseBll.ConvertCurrency(client.CurrencyId, currency,
                                  BaseHelpers.GetClientProductBalance(client.Id, product.Id))),
                        CurrencyCode = client.CurrencyId
                    };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(response))
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(JsonConvert.SerializeObject(fex.Detail.Message))
                };
                res.Content.Headers.Add("X_ErrorCode", fex.Detail.Id.ToString());
                res.Content.Headers.Add("X_ErrorMessage", fex.Detail.Message);
                return res;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);

                var res = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(JsonConvert.SerializeObject(ex.Message))
                };
                res.Content.Headers.Add("X_ErrorCode", Constants.Errors.GeneralException.ToString());
                res.Content.Headers.Add("X_ErrorMessage", ex.Message);
                return res;
            }
        }
    }
}