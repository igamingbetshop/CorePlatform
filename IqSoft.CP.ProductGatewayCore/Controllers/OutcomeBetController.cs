using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.OutcomeBet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using GetBalanceInput = IqSoft.CP.ProductGateway.Models.OutcomeBet.GetBalanceInput;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class OutcomeBetController : ControllerBase
    {

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "51.91.17.175",
            "51.91.73.174",
            "51.178.131.57",
            "51.210.36.141",
            "51.210.120.12",
            "51.210.122.137",
            "149.202.219.236",
            "193.70.9.91",
            "193.70.9.93",
            "51.210.121.184",
            "51.210.123.251",
            "51.210.124.25",
            "51.178.60.74",
            "51.210.37.182",
            "51.210.124.180"
        };

        [HttpPost]
        [Route("{partnerId}/api/OutcomeBet")]
        [Route("{partnerId}/api/Mascot")]
        public ActionResult ApiRequest(BaseInput input, [FromQuery]int providerId)
        {
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            var response = new BaseResponse
            {
                JsonRpc = "2.0",
                RequestId = input.RequestId
            };
            try
            {
               // Program.DbLogger.Info("input:  " + JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                switch (input.Method)
                {
                    case "Balance.Get":
                        var balanceInput = JsonConvert.DeserializeObject<BalanceInput>(JsonConvert.SerializeObject(input.Params));
                        var clientSession = ClientBll.GetClientProductSession(balanceInput.Context.SessionAlternativeId, Constants.DefaultLanguageId);
                        response.Result = new { Amount = Convert.ToInt32(GetBalance(Convert.ToInt32(balanceInput.PlayerId), clientSession) * 100) };
                        break;
                    case "getBalance":
                        var getBalanceInput = JsonConvert.DeserializeObject<GetBalanceInput>(JsonConvert.SerializeObject(input.Params));
                        var session = ClientBll.GetClientProductSession(getBalanceInput.SessionAlternativeId, Constants.DefaultLanguageId);
                        response.Result = new { balance = Convert.ToInt32(GetBalance(Convert.ToInt32(getBalanceInput.PlayerName), session) * 100) };
                        break;
                    case "Balance.Change":
                        var actionInput = JsonConvert.DeserializeObject<ActionInput>(JsonConvert.SerializeObject(input.Params));
                        response.Result = Action(actionInput, providerId);
                        break;
                    case "withdrawAndDeposit":
                        var withdrawAndDepositInput = JsonConvert.DeserializeObject<WithdrawAndDepositInput>(JsonConvert.SerializeObject(input.Params));
                        response.Result = WithdrawAndDeposit(withdrawAndDepositInput, providerId);
                        break;
                    case "rollbackTransaction":
                         var rollbackInput =JsonConvert.DeserializeObject<RollbackInput>(JsonConvert.SerializeObject(input.Params));
                        Rollback(rollbackInput, providerId);
                        break;
                    default: break;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.ErrorData = new Error
                {
                    Code = OutcomeBetHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                Program.DbLogger.Error(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.ErrorData = new Error
                {
                    Code = OutcomeBetHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                Program.DbLogger.Error(ex.Message);
            }
            return Ok(response);
        }

        private static decimal GetBalance(int clientId, SessionIdentity clientSession)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
            if (isExternalPlatformClient)
            {
                ClientBll.GetClientPlatformSession(client.Id, clientSession == null ? (long?)null : (clientSession.ParentId ?? 0));
                return ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
            }
            return BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
        }

        private static object Action(ActionInput actionInput, int providerId)
        {
            //var clientSession = ClientBll.GetClientProductSession(actionInput.Context.SessionAlternativeId, Constants.DefaultLanguageId);
            var win = actionInput.Operations.Where(x => x.Type.ToLower() == "win").FirstOrDefault();
            if (win != null)
                return DoBetWin(actionInput, providerId);
            else if (actionInput.Operations.Where(x => x.Type.ToLower() == "bet").FirstOrDefault() != null)
                return DoBet(actionInput, providerId);
            var balance = Convert.ToInt32(GetBalance(Convert.ToInt32(actionInput.PlayerId), null) * 100);
            var client = CacheManager.GetClientById(actionInput.PlayerId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

            return new { BalanceBefore = balance, BalanceAfter = balance };
        }

        private static object DoBet(ActionInput actionInput, int providerId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(actionInput.Context.SessionAlternativeId, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(actionInput.PlayerId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var beforBalance = Convert.ToInt32(GetBalance(client.Id, clientSession) * 100);
                    var product = CacheManager.GetProductByExternalId(providerId, actionInput.Context.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(actionInput.OPID, client.Id, providerId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = providerId,
                            ExternalProductId = actionInput.Context.GameId,
                            ProductId = product.Id,
                            TransactionId = actionInput.OPID,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        var amount = actionInput.Operations.Where(x => x.Type.ToLower() == "bet").Select(x => x.Amount).FirstOrDefault() / 100m;
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);

                        betDocument.State = (int)BetDocumentStates.Lost;
                        var recOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = providerId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = actionInput.Context.GameId,
                            ProductId = product.Id,
                            TransactionId = actionInput.OPID + "_win",
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Lost,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        recOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var doc = clientBl.CreateDebitsToClients(recOperationsFromProduct, betDocument, documentBl);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument);
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    (betDocument == null ? (long?)null : betDocument.Id), recOperationsFromProduct, doc[0]);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    return new { BalanceBefore = beforBalance, BalanceAfter = Convert.ToInt32(GetBalance(client.Id, clientSession) * 100) };
                }
            }
        }

        private static object DoBetWin(ActionInput actionInput, int providerId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(actionInput.Context.SessionAlternativeId, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(actionInput.PlayerId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var beforBalance = Convert.ToInt32(GetBalance(client.Id, clientSession) * 100);
                    var product = CacheManager.GetProductByExternalId(providerId, actionInput.Context.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var betDocument = documentBl.GetDocumentByExternalId(actionInput.OPID, client.Id, providerId,
                                                  partnerProductSetting.Id, (int)OperationTypes.Bet);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);

                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = providerId,
                            ExternalProductId = actionInput.Context.GameId,
                            ProductId = product.Id,
                            TransactionId = actionInput.OPID,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        var bet = actionInput.Operations.FirstOrDefault(x => x.Type.ToLower() == "bet");
                        var amount = bet == null ? 0 : bet.Amount / 100m;
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                    }
                    var winDocument = documentBl.GetDocumentByExternalId(actionInput.OPID, client.Id, providerId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        var amount = actionInput.Operations.Where(x => x.Type.ToLower() == "win").Select(x => x.Amount).FirstOrDefault() / 100m;
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = providerId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = actionInput.Context.GameId,
                            ProductId = betDocument.ProductId,
                            TransactionId = actionInput.OPID,
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
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0]);
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
                                Program.DbLogger.Error(JsonConvert.SerializeObject(message));
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
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
                    return new { BalanceBefore = beforBalance, BalanceAfter = Convert.ToInt32(GetBalance(client.Id, clientSession) * 100) };
                }
            }
        }

        private static object WithdrawAndDeposit(WithdrawAndDepositInput input, int providerId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.SessionAlternativeId, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerName));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var beforBalance = Convert.ToInt32(GetBalance(client.Id, clientSession) * 100);
                    var product = CacheManager.GetProductByExternalId(providerId, input.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var betDocument = documentBl.GetDocumentByExternalId(input.TransactionRef, client.Id, providerId,
                                                  partnerProductSetting.Id, (int)OperationTypes.Bet);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);

                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = providerId,
                            ExternalProductId = input.GameId,
                            ProductId = product.Id,
                            RoundId = input.GameRoundRef,
                            TransactionId = input.TransactionRef,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        var amount = input.Withdraw / 100m;
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                    }
                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionRef, client.Id, providerId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        var amount = input.Deposit != 0m ? input.Deposit / 100m : 0m;
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = providerId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = input.GameId,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.TransactionRef,
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
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0]);
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
                                Program.DbLogger.Error(JsonConvert.SerializeObject(message));
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
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
                    return new { newBalance = Convert.ToInt32(GetBalance(client.Id, clientSession) * 100), transactionId = betDocument.Id.ToString() };
                }
            }
        }

        private static void Rollback(RollbackInput input, int providerId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(providerId, input.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var clientSession = ClientBll.GetClientProductSession(input.SessionAlternativeId, Constants.DefaultLanguageId, checkExpiration: false);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = providerId,
                        TransactionId = input.TransactionRef,
                        ExternalProductId = product.ExternalId,
                        ProductId = product.Id
                    };
                    try
                    {
                        documentBl.RollbackProductTransactions(operationsFromProduct);
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        Program.DbLogger.Info("Rollback Input: " + JsonConvert.SerializeObject(input));
                        if (fex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked && fex.Detail.Id != Constants.Errors.DocumentNotFound)
                            throw;
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(Convert.ToInt32(input.PlayerName));
                }
            }
        }

    }
}