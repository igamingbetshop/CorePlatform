using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using IqSoft.CP.ProductGateway.Models.Nucleus;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Cache;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class NucleusController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Nucleus).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "",
        };


        [HttpPost]
        [Route("{partnerId}/api/Nucleus/Authenticate")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult CheckSession(BaseInput input)
        {
            string xmlResponse;

            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
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
                        BALANCE = (int)Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2) * 100
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/getAccountInfo")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult GetAccountInfo(BaseInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                if (!int.TryParse(input.userId, out int clientId))
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/getBalance")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult GetBalance(BaseInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
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
                        BALANCE = (int)Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2) * 100
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/betResult")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult DoBetWin(BaseInput input)
        {
            var xmlResponse = string.Empty;
            Program.DbLogger.Info(JsonConvert.SerializeObject(input));

            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                            BALANCE = (int)Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2) * 100
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }


        private long DoBet(string roundId, string TransactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    return betDocument.Id;
                }
            }
        }

        private void DoWin(long betDocumentId, string transactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                                ExternalOperationId = null,
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
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult Rollback(BaseInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
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
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                        BaseHelpers.BroadcastBalance(client.Id);
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/Nucleus/bonusWin")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult FreeBetWin(BonusInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
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
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.transactionId + "_BonusWin",
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
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
                        BALANCE = (int)Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2) * 100
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }
        [HttpPost]
        [Route("{partnerId}/api/Nucleus/bonusRelease")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult BonusWin(BonusInput input)
        {
            string xmlResponse;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
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

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.transactionId+ "_BonusWin",
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
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
                        BALANCE = (int)Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2) * 100
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

                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
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
                Program.DbLogger.Error(ex);
                xmlResponse = CommonFunctions.ToXML(response);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationXml;
            return Ok(xmlResponse);
        }
    }
}