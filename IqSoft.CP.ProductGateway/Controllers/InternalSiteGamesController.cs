using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using IqSoft.CP.ProductGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.ProductGateway.Models.IqSoft;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Filters;
using System.IO;
using System.Web;
using IqSoft.CP.Common.Models.Document;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class InternalSiteGamesController : ApiController
    {
        private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Internal).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Internal);

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/Authorization")]
        public IHttpActionResult Authorization(int partnerId, AuthorizationInput input)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    BllClient client = null;
                    int productId = Constants.SportsbookProductId;
                    string languageId = Constants.DefaultLanguageId;
                    string newToken = input.Token;
                    var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                    WebApiApplication.DbLogger.Info($"InputString: {bodyStream.ReadToEnd()}");
                    try
                    {
                        client = clientBl.ProductsAuthorization(input.Token, out newToken, out productId, out languageId, true);
                    }
                    catch(FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail.Id == Constants.Errors.SessionNotFound && input.ProductId.HasValue)
                        {
                            client = clientBl.PlatformAuthorization(input.Token, out SessionIdentity session);
                            var product = CacheManager.GetProductByExternalId(providerId, input.ProductId.Value.ToString());
                            var s = new SessionIdentity
                            {
                                Id = client.Id,
                                LanguageId = input.LanguageId,
                                ProductId = product.Id,
                                LoginIp = session.LoginIp,
                                DeviceType = session.DeviceType,
                                ParentId = session.SessionId,
                                SessionId = 0
                            };
                            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(s));
                            newToken = clientBl.CreateNewProductToken(s);
                            languageId = session.LanguageId;
                        }
                        else 
                            throw;
                    }
                    BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                    bool isShopWallet = false;
                    var ps = CacheManager.GetClientPlatformSession(client.Id, null);
                    if(ps.AccountId != null)
                    {
                        var accounts = clientBl.GetClientAccounts(client.Id, false);
                        var account = accounts.FirstOrDefault(x => x.Id == ps.AccountId);
                        if (account != null && account.BetShopId != null)
                            isShopWallet = true;
                    }

                    var response = client.MapToAuthorizationOutput(newToken, isShopWallet);
                    if (client.RegionId > 0)
                    {
                        var countryId = BLL.Helpers.CommonHelpers.GetCountryId(client.RegionId, languageId);
                        if(countryId != null)
                           response.CountryCode = CacheManager.GetRegionById(countryId.Value, languageId).IsoCode;
                    }
                    response.AvailableBalance = BaseHelpers.GetClientProductBalance(client.Id, productId);
                    response.DepositCount = CacheManager.GetClientDepositCount(client.Id);

                    if (productId == Constants.SportsbookProductId)
                    {
                        response.Bonuses = new List<ApiBonus>();
                        var bonuses = clientBl.GetClientActiveBonuses(client.Id, (int)BonusTypes.CampaignFreeBet, languageId);
                        foreach (var b in bonuses)
                        {
                            response.Bonuses.Add(new ApiBonus
                            {
                                Id = b.Id,
                                BonusId = b.BonusId,
                                BonusType = b.Type,
                                Condition = b.Condition,
                                Name = b.Name,
                                AllowSplit = b.AllowSplit ?? false,
                                Amount = b.RemainingCredit ?? b.BonusPrize
                            });
                        }
                    }
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} " +
                                                 $"Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/GetBalance")]
        public IHttpActionResult GetBalance(DAL.Models.Integration.ProductsIntegration.GetBalanceInput input)
        {
            try
            {
                var clientSession = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var currencyId = string.IsNullOrWhiteSpace(input.CurrencyId) ? client.CurrencyId : input.CurrencyId;
                    var response = new DAL.Models.Integration.ProductsIntegration.GetBalanceOutput
                    {
                        CurrencyId = currencyId
                    };
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        var externalBalance = Math.Floor(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) * 100) / 100;
                        response.AvailableBalance = BaseBll.ConvertCurrency(client.CurrencyId, currencyId, externalBalance);
                    }
                    else
                    {
                        response.AvailableBalance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                    }
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/GetSessionInfo")]
        public IHttpActionResult GetSessionInfo(DAL.Models.Integration.ProductsIntegration.GetBalanceInput input)
        {
            try
            {
                var clientSession = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                {
                    var response = clientBl.GetSessionInfo(input.CurrencyId);
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} " +
                                                 $"Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/Credit")]
        public IHttpActionResult Credit(ApiFinOperationInput input)
        {
            try
            {
                var session = CheckClientSession(input.Token, true);
                using (var documentBl = new DocumentBll(session, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var response = new FinOperationOutput
                        {
                            OperationItems = new List<FinOperationOutputItem>()
                        };
                        var typeId = (input.TypeId == null
                            ? null
                            : (int?)IqSoftHelpers.BetTypesMapping.First(x => x.Key == input.TypeId.Value).Value);
                        int? deviceType = session.DeviceType;
                        var client = CacheManager.GetClientById(session.Id);
                        if (client.CurrencyId != input.CurrencyId)
                            input.Amount = BaseBll.ConvertCurrency(input.CurrencyId, client.CurrencyId, input.Amount);
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = session.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            GameProviderId = providerId,
                            ExternalOperationId = null,
                            ExternalProductId = input.GameId + (input.UnitId == null ? string.Empty : ("_" + input.UnitId)),
                            TransactionId = input.TransactionId,
                            OperationTypeId = input.OperationTypeId,
                            Info = input.Info,
                            TypeId = typeId,
                            State = input.BetState,
                            SelectionsCount = input.SelectionsCount,
                            TicketInfo = input.Info,
                            BonusId = input.BonusId,
                            OperationItems = new List<OperationItemFromProduct>
                            {
                                new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = input.Amount,
                                    DeviceTypeId = deviceType,
                                    Type = input.Type,
                                    PossibleWin = input.PossibleWin
                                }
                            }
                        };

                        try
                        {
                            var document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                BarCode = document.Barcode,
                                BetId = document.Id.ToString(),
                                ClientId = document.ClientId.Value.ToString(),
                                BonusId = document.BonusId
                            });
                            decimal balance = 0;
                            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                        session.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
                                    BaseHelpers.BroadcastBalance(client.Id, balance);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex.Message);
                                    documentBl.RollbackProductTransactions(operationsFromProduct);
                                    throw;
                                }
                            }
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                                balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
                            }
                            response.OperationItems[0].Balance = balance;
                            response.OperationItems[0].AccountId = document.Transactions.Where(x => x.Type == (int)TransactionTypes.Credit).FirstOrDefault()?.AccountId;
                        }
                        catch (FaultException<BllFnErrorType> ex)
                        {
                            if (ex.Detail.Id != Constants.Errors.ClientMaxLimitExceeded &&
                                ex.Detail.Id != Constants.Errors.PartnerProductLimitExceeded)
                                throw;
                            response.ResponseCode = ex.Detail.Id;
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                CurrentLimit = ex.Detail.DecimalInfo == null ? 0 : ex.Detail.DecimalInfo.Value
                            });
                            WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(response));
                        }
                        return Ok(response);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} " +
                                                 $"Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/Debit")]
        public IHttpActionResult Debit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                return ProcessDebit(partnerId, input);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  " +
                                                 $"Input: {bodyStream.ReadToEnd()}  Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {               
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                }; 
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        private IHttpActionResult ProcessDebit(int partnerId, ApiFinOperationInput input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var response = new FinOperationOutput
                    {
                        OperationItems = new List<FinOperationOutputItem>()
                    };
                    SessionIdentity clientSession = null;
                    if (!string.IsNullOrEmpty(input.Token))
                     clientSession = CheckClientSession(input.Token, false);
                    var client = input.ClientId > 0 ? CacheManager.GetClientById(input.ClientId) : CacheManager.GetClientByUserName(partnerId, input.UserName);
                    if (client.CurrencyId != input.CurrencyId)
                        input.Amount = BaseBll.ConvertCurrency(input.CurrencyId, client.CurrencyId, input.Amount);
                    var gameProviderId = providerId;
                    var product = CacheManager.GetProductByExternalId(gameProviderId, input.GameId + (input.UnitId == null ? string.Empty : ("_" + input.UnitId)));

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                        product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw new ArgumentNullException(Constants.Errors.ProductNotFound.ToString());
                    Document creditTransaction = null;
                    if (input.OperationTypeId != (int)OperationTypes.WageringBonus)
                    {
                        if (string.IsNullOrEmpty(input.CreditTransactionId) && input.OperationTypeId == (int)OperationTypes.BonusWin)
                        {
                            var betTransactionId = string.Format("BonusWin_{0}", input.TransactionId);
                            creditTransaction = documentBl.GetDocumentByExternalId(betTransactionId,
                                client.Id, gameProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                            if (creditTransaction != null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                            var betOperationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession?.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = product.GameProviderId.Value,
                                ProductId = product.Id,
                                TransactionId = betTransactionId,
                                OperationTypeId = (int)OperationTypes.Bet,
                                State = (int)BetDocumentStates.Uncalculated,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            creditTransaction = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);

                            input.OperationTypeId = (int)OperationTypes.Win;
                        }
                        else
                        {
                            creditTransaction = documentBl.GetDocumentByExternalId(input.CreditTransactionId,
                                client.Id, gameProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                            if (creditTransaction == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        }
                        if (input.BetState.HasValue)
                            creditTransaction.State = input.BetState.Value;

                        try
                        {
                            var documentInfo = JsonConvert.DeserializeObject<DocumentInfo>(creditTransaction.Info);
                            if(documentInfo != null && documentInfo.BonusId > 0)
                            {
                                var bonus = CacheManager.GetBonusById(documentInfo.BonusId);
                                if (bonus != null)
                                {
                                    if (bonus.Type == (int)BonusTypes.CampaignFreeBet && bonus.MaxAmount != null)
                                    {
                                        var partner = CacheManager.GetPartnerById(client.PartnerId);
                                        input.Amount = Math.Min(input.Amount, BaseBll.ConvertCurrency(partner.CurrencyId, client.CurrencyId, bonus.MaxAmount.Value));
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession?.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = input.RoundId,
                        GameProviderId = providerId,
                        OperationTypeId = input.OperationTypeId,
                        ExternalOperationId = null,
                        ExternalProductId = input.GameId + (input.UnitId == null ? string.Empty : ("_" + input.UnitId)),
                        TransactionId = input.TransactionId,
                        CreditTransactionId = (creditTransaction == null ? (long?)null : creditTransaction.Id),
                        Info = input.Info,
                        State = input.BetState,
                        TicketInfo = input.Info,
                        IsFreeBet = input.IsFreeBet,
                        OperationItems = new List<OperationItemFromProduct>
                        {
                            new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount
                            }
                        }
                    };
                    if (input.OperationTypeId == (int)OperationTypes.CashOut)
                    {
                        operationsFromProduct.State = (int)BetDocumentStates.Cashouted;
                        if (creditTransaction != null)
                            creditTransaction.State = (int)BetDocumentStates.Cashouted;
                    }
                    var documents = clientBl.CreateDebitsToClients(operationsFromProduct, creditTransaction, documentBl);
                    foreach (var win in documents)
                    {
                        var outputItem =
                            new FinOperationOutputItem
                            {
                                BarCode = win.Barcode,
                                BetId = win.Id.ToString(),
                                ClientId = win.ClientId.Value.ToString()
                            };
                        response.OperationItems.Add(outputItem);
                    }
                    decimal balance = 0;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            balance = ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                (creditTransaction == null ? (long?)null : creditTransaction.Id), operationsFromProduct, documents[0], WebApiApplication.DbLogger);
                        }
                        catch (FaultException<BllFnErrorType> ex)
                        {
                            var message = ex.Detail == null
                                ? new ResponseBase
                                {
                                    ResponseCode = Constants.Errors.GeneralException,
                                    Description = ex.Message
                                }
                                : new ResponseBase
                                {
                                    ResponseCode = ex.Detail.Id,
                                    Description = ex.Detail.Message
                                };
                            WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(message));
                            documentBl.RollbackProductTransactions(operationsFromProduct);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex.Message);
                            documentBl.RollbackProductTransactions(operationsFromProduct);
                            throw;
                        }
                    }
                    else
                    {
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = input.Amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                        balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                    }
                    response.OperationItems[0].Balance = balance;
                    return Ok(response);
                }
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/RollBack")]
        public IHttpActionResult RollBack(ApiFinOperationInput input)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientSession = CheckClientSession(input.Token, false);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = providerId,
                        ExternalProductId = input.GameId + (input.UnitId == null ? string.Empty : ("_" + input.UnitId)),
                        TransactionId = input.TransactionId,
                        ExternalOperationId = null,
                        Info = input.Info,
                        OperationTypeId = input.OperationTypeId
                    };
                    var rollbackDocument = documentBl.RollbackProductTransactions(operationsFromProduct);
                    var client = CacheManager.GetClientById(rollbackDocument[0].ClientId.Value);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                        operationsFromProduct, rollbackDocument[0], WebApiApplication.DbLogger);
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    return Ok(new ResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  " +
                                                 $"Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/GetPartnerLanguages")]
        public IHttpActionResult GetPartnerLanguages(int partnerId)
        {
            try
            {
                using (var languageBl = new LanguageBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var partnerLanguages = languageBl.GetPartnerLanguages(partnerId);
                    return Ok(new PartnerLanguagesOutput { Languages = partnerLanguages.Select(x => new ApiLanguage { Id = x.LanguageId, Name = x.Language.Name }).ToList() });
                }
            }
            catch (Exception ex)
            {
                var response = new PartnerLanguagesOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message,
                    Languages = new List<ApiLanguage>()
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }


        [HttpPost]
        [Route("{partnerId}/api/InternalSiteGames/GetFilteredAccounts")]
        public IHttpActionResult GetFilteredAccounts(DAL.Models.Integration.ProductsIntegration.GetBalanceInput input)
        {
            try
            {
                var clientSession = CheckClientSession(input.Token, true);
                var response = new List<long>();
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                {
                    var ps = CacheManager.GetClientPlatformSession(clientSession.Id, null);
                    if(ps.AccountId != null)
                    {
                        var account = clientBl.GetAccount(ps.AccountId.Value);
                        if ((account.TypeId == (int)AccountTypes.ClientUnusedBalance || account.TypeId == (int)AccountTypes.ClientUsedBalance) &&
                            account.BetShopId == null && account.PaymentSystemId == null)
                        {
                            var accounts = clientBl.GetfnAccounts(new FilterfnAccount
                            {
                                ObjectId = ps.ClientId,
                                ObjectTypeId = (int)ObjectTypes.Client
                            });
                            response = accounts.Where(x => (x.TypeId == (int)AccountTypes.ClientUnusedBalance ||
                                x.TypeId == (int)AccountTypes.ClientUsedBalance) && x.BetShopId == null && x.PaymentSystemId == null).Select(x => x.Id).ToList();
                        }
                        else
                            response.Add(ps.AccountId.Value);
                    }

                    return Ok(new Models.IqSoft.ApiResponseBase { ResponseObject = JsonConvert.SerializeObject(response) });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}" +
                                                 $"Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }
        private SessionIdentity CheckClientSession(string token, bool checkExpiration)
        {
            var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, checkExpiration);
            return session;
        }
    }
}
