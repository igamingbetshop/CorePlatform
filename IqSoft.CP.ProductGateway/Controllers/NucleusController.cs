using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.Nucleus;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Web.Http.Cors;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NucleusController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Nucleus).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Nucleus);

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/Authenticate")]
        public HttpResponseMessage CheckSession(BaseInput input)
        {
            string xmlResponse;

            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                var hash = CommonFunctions.ComputeMd5(input.token + apiKey).ToLower();
                if (hash != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var responseObject = new EXTSYSTEM
                {
                    REQUEST = new EXTSYSTEMREQUEST
                    {
                        TOKEN = input.token,
                        HASH = hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new EXTSYSTEMRESPONSE
                    {
                        RESULT = "OK",
                        USERID = client.Id.ToString(),
                        USERNAME = client.UserName,
                        CURRENCY = client.CurrencyId,
                        BALANCE = (int)Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2) * 100
                    }
                };
                xmlResponse = CommonFunctions.ToXML(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ERROREXTSYSTEM
                {
                    REQUEST = new EXTSYSTEMREQUEST
                    {
                        TOKEN = input.token,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new ERROREXTSYSTEM
                {
                    REQUEST = new EXTSYSTEMREQUEST
                    {
                        TOKEN = input.token,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/getAccountInfo")]
        public HttpResponseMessage GetAccountInfo(BaseInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                if(!int.TryParse(input.userId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                var hash = CommonFunctions.ComputeMd5(input.userId + apiKey).ToLower();
                if (hash != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var responseObject = new Models.Nucleus.AccountInfo.EXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.AccountInfo.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        HASH = hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.AccountInfo.EXTSYSTEMRESPONSE
                    {
                        RESULT = "OK",                     
                        USERNAME = client.UserName,
                        FIRSTNAME = client.FirstName,
                        LASTNAME = client.LastName,
                        EMAIL = client.Email,
                        CURRENCY = client.CurrencyId
                    }
                };
                xmlResponse = CommonFunctions.ToXML(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new Models.Nucleus.AccountInfo.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.AccountInfo.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.AccountInfo.ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new Models.Nucleus.AccountInfo.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.AccountInfo.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.AccountInfo.ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/getBalance")]
        public HttpResponseMessage GetBalance(BaseInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                if (!int.TryParse(input.userId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                var responseObject = new Models.Nucleus.Balance.EXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Balance.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Balance.EXTSYSTEMRESPONSE
                    {
                        RESULT = "OK",
                        BALANCE = (int)Math.Round(BaseHelpers.GetClientProductBalance(client.Id, 0), 2) * 100
                    }
                };
                xmlResponse = CommonFunctions.ToXML(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new Models.Nucleus.Balance.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Balance.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Balance.ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new Models.Nucleus.Balance.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Balance.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Balance.ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/betResult")]
        public HttpResponseMessage DoBetWin(BaseInput input)
        {
            var xmlResponse = string.Empty;
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));

            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                    var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                    var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}{5}{6}", input.userId, input.bet, input.win,
                                                                                                 input.isRoundFinished.HasValue ? input.isRoundFinished.ToString().ToLower() : string.Empty,
                                                                                                 input.roundId, input.gameId, apiKey)).ToLower();
                    if (hash != input.hash.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    long betDocumentId = 0;
                    if (!string.IsNullOrEmpty(input.bet))
                    {
                        var bet = input.bet.Split('|');
                        betDocumentId = DoBet(input.roundId, bet[1], Convert.ToDecimal(bet[0]) /100, clientSession, client);
                        if (!string.IsNullOrEmpty(input.win))
                        {
                            var win = input.win.Split('|');
                            DoWin(betDocumentId, win[1], Convert.ToDecimal(win[1]) / 100, clientSession, client);
                        }
                    }
                    if (input.isRoundFinished.HasValue && input.isRoundFinished.Value)
                    {
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.roundId, ProviderId,
                                                                                     client.Id, (int)BetDocumentStates.Uncalculated);

                            foreach (var b in betDocuments)
                            {
                                betDocumentId = b.Id;
                                DoWin(b.Id, "win_" + b.Id, 0, clientSession, client);
                            }
                        }
                    }
                    var responseObject = new Models.Nucleus.Bet.EXTSYSTEM
                    {
                        REQUEST = new Models.Nucleus.Bet.EXTSYSTEMREQUEST
                        {
                            USERID = input.userId,
                            BET = input.bet,
                            ROUNDID = input.roundId,
                            GAMEID = input.gameId,
                            ISROUNDFINISHED = input.isRoundFinished ?? false,
                            HASH = hash
                        },
                        TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                        RESPONSE = new Models.Nucleus.Bet.EXTSYSTEMRESPONSE
                        {
                            RESULT = "OK",
                            EXTSYSTEMTRANSACTIONID = betDocumentId,
                            BALANCE = (int)Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2) * 100
                        }
                    };
                    xmlResponse = CommonFunctions.ToXML(responseObject);
                 
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new Models.Nucleus.Bet.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Bet.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BET = input.bet,
                        ROUNDID = input.roundId,
                        GAMEID = input.gameId,
                        ISROUNDFINISHED = input.isRoundFinished ?? false,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Bet.ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new Models.Nucleus.Bet.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Bet.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BET = input.bet,
                        ROUNDID = input.roundId,
                        GAMEID = input.gameId,
                        ISROUNDFINISHED = input.isRoundFinished ?? false,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Bet.ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }


        private long DoBet(string roundId, string TransactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(TransactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = clientSession.ProductId,
                            RoundId = roundId,
                            TransactionId = TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    return betDocument.Id;
                }
            }
        }

        private void DoWin(long betDocumentId, string transactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentById(betDocumentId);
                    if (betDocument != null)
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = transactionId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = product.ExternalId,
                                ProductId = betDocument.ProductId,
                                TransactionId = transactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
                            });

                            clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new Common.Models.WebSiteModels.ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = amount,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                    }
                }
            }
        }
        [HttpPost]
        [Route("{partnerId}/api/Nucleus/refundBet")]
        public HttpResponseMessage Rollback(BaseInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                if (!int.TryParse(input.userId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                var hash = CommonFunctions.ComputeMd5(input.userId + input.casinoTransactionId + apiKey).ToLower();
                if (hash != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                long transactionId = 0;
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            GameProviderId = ProviderId,
                            TransactionId = input.casinoTransactionId
                        };
                        try
                        {
                            transactionId =  documentBl.RollbackProductTransactions(operationsFromProduct)[0].Id;
                        }
                        catch (FaultException<BllFnErrorType>)
                        {
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    }
                }

                var responseObject = new Models.Nucleus.Refund.EXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Refund.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        HASH = hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Refund.EXTSYSTEMRESPONSE
                    {
                        RESULT = "OK",
                        EXTSYSTEMTRANSACTIONID = transactionId
                    }
                };
                xmlResponse = CommonFunctions.ToXML(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new Models.Nucleus.Refund.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Refund.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        HASH = input.hash,
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Refund.ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new Models.Nucleus.Refund.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.Refund.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.Refund.ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/bonusWin")]
        public HttpResponseMessage FreeBetWin(BonusInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                if (!int.TryParse(input.userId, out int clientId) || Convert.ToInt32(input.amount) <= 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", input.userId, input.bonusId, input.amount, apiKey)).ToLower();
                if (hash != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(input.transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.transactionId + "_FreeBet",
                                OperationTypeId = (int)OperationTypes.Bet,
                                State = (int)BetDocumentStates.Won,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.transactionId + "_BonusWin",
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.transactionId,
                                CreditTransactionId = betDocument.Id,
                                State = (int)BetDocumentStates.Won,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = Convert.ToInt32(input.amount)
                            });
                            clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new Common.Models.WebSiteModels.ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = Convert.ToInt32(input.amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                    }
                }
                var responseObject = new Models.Nucleus.BonusWin.EXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.BonusWin.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BONUSID = input.bonusId,
                        AMOUNT = input.amount,
                        TRANSACTIONID = input.transactionId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.BonusWin.EXTSYSTEMRESPONSE
                    {
                        RESULT = "OK",
                        BALANCE = (int)Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2) * 100
                    }
                };
                xmlResponse = CommonFunctions.ToXML(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new Models.Nucleus.BonusWin.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.BonusWin.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BONUSID = input.bonusId,
                        AMOUNT = input.amount,
                        TRANSACTIONID = input.transactionId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.BonusWin.ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new Models.Nucleus.BonusWin.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.BonusWin.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BONUSID = input.bonusId,
                        AMOUNT = input.amount,
                        TRANSACTIONID = input.transactionId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.BonusWin.ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }
        [HttpPost]
        [Route("{partnerId}/api/Nucleus/bonusRelease")]
        public HttpResponseMessage BonusWin(BonusInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                if (!int.TryParse(input.userId, out int clientId) || Convert.ToInt32(input.amount) <= 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.NucleusApiKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", input.userId, input.bonusId, input.amount, apiKey)).ToLower();
                if (hash != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, "BonusWin");
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);               

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(input.transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.transactionId + "_BonusBet",
                                OperationTypeId = (int)OperationTypes.Bet,
                                State = (int)BetDocumentStates.Won,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.transactionId+ "_BonusWin",
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.transactionId,
                                CreditTransactionId = betDocument.Id,
                                State = (int)BetDocumentStates.Won,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = Convert.ToInt32(input.amount)
                            });
                            clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new Common.Models.WebSiteModels.ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = Convert.ToInt32(input.amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                    }
                }
                var responseObject = new Models.Nucleus.BonusWin.EXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.BonusWin.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BONUSID = input.bonusId,
                        AMOUNT = input.amount,
                        TRANSACTIONID = input.transactionId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.BonusWin.EXTSYSTEMRESPONSE
                    {
                        RESULT = "OK",
                        BALANCE = (int)Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2) * 100
                    }
                };
                xmlResponse = CommonFunctions.ToXML(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new Models.Nucleus.BonusWin.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.BonusWin.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BONUSID = input.bonusId,
                        AMOUNT = input.amount,
                        TRANSACTIONID = input.transactionId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.BonusWin.ERRORRESPONSE
                    {
                        RESULT = fex.Detail.Message,
                        CODE = NucleusHelpers.GetErrorCode(fex.Detail.Id)
                    }
                };

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                xmlResponse = CommonFunctions.ToXML(response);
            }
            catch (Exception ex)
            {
                var response = new Models.Nucleus.BonusWin.ERROREXTSYSTEM
                {
                    REQUEST = new Models.Nucleus.BonusWin.EXTSYSTEMREQUEST
                    {
                        USERID = input.userId,
                        BONUSID = input.bonusId,
                        AMOUNT = input.amount,
                        TRANSACTIONID = input.transactionId,
                        HASH = input.hash
                    },
                    TIME = DateTime.UtcNow.ToString("dd MMM yyy HH:mm:ss"),
                    RESPONSE = new Models.Nucleus.BonusWin.ERRORRESPONSE
                    {
                        RESULT = ex.Message,
                        CODE = NucleusHelpers.GetErrorCode(Constants.Errors.GeneralException)
                    }
                };
                WebApiApplication.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlResponse, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }
    }
}