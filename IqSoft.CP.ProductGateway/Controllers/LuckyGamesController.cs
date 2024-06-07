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
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class LuckyGamesController : ApiController
    {
        private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyGames).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.LuckyGames);
        [HttpPost]
        [Route("{partnerId}/api/LuckyGames/Authorization")]
        public IHttpActionResult Authorization(int partnerId, AuthorizationInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var client = clientBl.ProductsAuthorization(input.Token, out string newToken, out int productId, out string languageId, true);
                    BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                    var response = client.MapToAuthorizationOutput(newToken, false);
                    response.AvailableBalance = BaseHelpers.GetClientProductBalance(client.Id, productId);
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGames/GetBalance")]
        public IHttpActionResult GetBalance(int partnerId, GetBalanceInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var currencyId = string.IsNullOrWhiteSpace(input.CurrencyId) ? client.CurrencyId : input.CurrencyId;
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                    var response = new GetBalanceOutput
                    {
                        CurrencyId = client.CurrencyId,
                        AvailableBalance = balance
                    };
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGames/Credit")]
        public IHttpActionResult Credit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var session = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(session, WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
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

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = session.SessionId,
                            CurrencyId = input.CurrencyId,
                            RoundId = input.RoundId,
                            GameProviderId = providerId,
                            ExternalProductId = input.GameId,
                            TransactionId = input.TransactionId,
                            OperationTypeId = input.OperationTypeId,
                            TypeId = typeId,
                            State = input.BetState,
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
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                BarCode = document.Barcode,
                                BetId = document.Id.ToString(),
                                ClientId = document.ClientId.Value.ToString()
                            });
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
                            WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input), ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGames/Debit")]
        public IHttpActionResult Debit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input), ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGames/RollBack")]
        public IHttpActionResult RollBack(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = CheckClientSession(input.Token, false);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId= clientSession.SessionId,
                        GameProviderId = providerId,
                        ExternalProductId = input.GameId,
                        TransactionId = input.TransactionId,
                        Info = input.Info
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(input.ClientId);
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
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
                    var clientSession = CheckClientSession(input.Token, false);
                    var client = input.ClientId > 0 ? CacheManager.GetClientById(input.ClientId) : CacheManager.GetClientByUserName(partnerId, input.UserName);
                    var gameProviderId = providerId;
                    var product = CacheManager.GetProductByExternalId(gameProviderId, input.GameId);

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                        product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw new ArgumentNullException(Constants.Errors.ProductNotFound.ToString());

                    Document creditTransaction = null;
                    if (input.OperationTypeId != (int)OperationTypes.WageringBonus)
                    {
                        creditTransaction = documentBl.GetDocumentByExternalId(input.CreditTransactionId,
                            client.Id, gameProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (creditTransaction == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        if (input.BetState != null)
                            creditTransaction.State = input.BetState.Value;
                    }
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = input.CurrencyId,
                        RoundId = input.RoundId,
                        GameProviderId = providerId,
                        OperationTypeId = input.OperationTypeId,
                        ExternalProductId = input.GameId,
                        TransactionId = input.TransactionId,
                        CreditTransactionId = (creditTransaction == null ? (long?)null : creditTransaction.Id),
                        Info = input.Info,
                        State = input.BetState,
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
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);

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
                    return Ok(response);
                }
            }
        }

        private SessionIdentity CheckClientSession(string token, bool checkExpiration)
        {
            var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, checkExpiration);
            return session;
        }
    }
}
