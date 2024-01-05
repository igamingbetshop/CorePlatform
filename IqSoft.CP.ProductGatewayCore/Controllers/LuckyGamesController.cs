using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
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

using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class LuckyGamesController : ControllerBase
    {
        private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyGames).Id;
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "54.92.136.55",
            "3.220.10.28",
            "34.195.181.53",
            "52.54.242.193",
            "107.21.65.21",
            "100.75.78.123",
            "172.217.192.132",
            "52.202.3.211"
        };

        [HttpPost]
        [Route("{partnerId}/api/LuckyGames/Authorization")]
        public IActionResult Authorization(AuthorizationInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var client = clientBl.ProductsAuthorization(input.Token, out string newToken, out int productId, out string languageId, true);
                    BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                    var response = client.MapToAuthorizationOutput(newToken);
                    var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client, client.Id, client.CurrencyId);
                    response.AvailableBalance = balance.AvailableBalance;
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
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
        public IActionResult GetBalance(GetBalanceInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var clientSession = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(clientSession, Program.DbLogger))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var currencyId = string.IsNullOrWhiteSpace(input.CurrencyId) ? client.CurrencyId : input.CurrencyId;
                    var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client, client.Id, currencyId);
                    var response = new GetBalanceOutput
                    {
                        CurrencyId = balance.CurrencyId,
                        AvailableBalance = balance.AvailableBalance
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
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
        public IActionResult Credit(ApiFinOperationInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var session = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(session, Program.DbLogger))
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
                            ExternalOperationId = null,
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
                            var document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                BarCode = document.Barcode,
                                BetId = document.Id,
                                ClientId = document.ClientId.Value
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
                            Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input), ex);
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
        public IActionResult Debit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input), ex);
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
        public IActionResult RollBack(ApiFinOperationInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var clientSession = CheckClientSession(input.Token, false);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId= clientSession.SessionId,
                        GameProviderId = providerId,
                        ExternalProductId = input.GameId,
                        TransactionId = input.TransactionId,
                        ExternalOperationId = null,
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        private IActionResult ProcessDebit(int partnerId, ApiFinOperationInput input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                        ExternalOperationId = null,
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
                    BaseHelpers.BroadcastBalance(client.Id);

                    foreach (var win in documents)
                    {
                        var outputItem =
                            new FinOperationOutputItem
                            {
                                BarCode = win.Barcode,
                                BetId = win.Id,
                                ClientId = win.ClientId.Value
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
