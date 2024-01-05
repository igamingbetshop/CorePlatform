using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SoftSwiss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Action = IqSoft.CP.ProductGateway.Models.SoftSwiss.Action;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class SoftSwissController : ControllerBase
    {

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftSwiss).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            ""
        };
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Constants.Currencies.IranianTuman,
            Constants.Currencies.IranianRial
        };
        private static Regex Rgx = new Regex(@"\d+$");

        [HttpPost]
        [Route("{partnerId}/api/SoftSwiss/play")]
        public ActionResult ApiRequest(BaseInput input)
        {
            var output = new BaseOutput();
            try
            {
                var bodyStream = new StreamReader(Request.Body);
                var inputString = bodyStream.ReadToEnd();
                Program.DbLogger.Info(inputString);

                // BaseBll.CheckIp(WhitelistedIps);
                if (string.IsNullOrEmpty(input.GameId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, Rgx.Replace(input.GameId, string.Empty));
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientSessionByProductId(input.ClientId, product.Id);
                var client = CacheManager.GetClientById(input.ClientId);
                var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SoftSwissAuthToken);
                string authHeader = Request.Headers["X-REQUEST-SIGN"];
                var hashString = CommonFunctions.ComputeHMACSha256(inputString, authToken);
                if (hashString.ToLower() != authHeader.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.RoundId = input.RoundId;
                output.Balance = Convert.ToInt32(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));

                if (input.Actions != null)
                {
                    var transactions = new List<Transaction>();
                    using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        foreach (var ac in input.Actions)
                        {
                            if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                                ac.Amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, ac.Amount);
                            if (ac.ActionName == "bet")
                                transactions.Add(DoBet(input, ac, client, clientSession));
                            else if (ac.ActionName == "win")
                                transactions.Add(DoWin(input, ac, client, clientSession));
                        }
                        scope.Complete();
                    }
                    output.Transactions = transactions;
                }
                if (input.Finished)
                {
                    FinalizeRound(client, input.RoundId, product.Id, clientSession.Id);
                }
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
                output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                output.Message = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Message = ex.Message;
            }

            return Ok(output);
        }

        private Transaction DoBet(BaseInput actionInput, Action betAction, BllClient client, DAL.ClientSession sessionIdentity)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(sessionIdentity.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(betAction.ActionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = sessionIdentity.Id,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = actionInput.GameId,
                            ProductId = product.Id,
                            TransactionId = betAction.ActionId,
                            RoundId = actionInput.RoundId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = betAction.Amount / 100,
                            DeviceTypeId = sessionIdentity.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl/*, out LimitInfo info*/);
                        //BaseHelpers.BroadcastBetLimit(info);
                        var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.SoftSwissRollback, betAction.ActionId);
                        if (!string.IsNullOrEmpty(rollback))
                        {
                            var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                            {
                                GameProviderId = ProviderId,
                                TransactionId = betAction.ActionId,
                                ExternalProductId = product.ExternalId,
                                ProductId = product.Id
                            };
                            try
                            {
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                            }
                            catch {; }
                        }
                    }
                    return new Transaction
                    {
                        ActionId = betAction.ActionId,
                        TxId = betDocument.Id.ToString()
                    };
                }
            }
        }

        private static Transaction DoWin(BaseInput actionInput, Action winAction, BllClient client, DAL.ClientSession sessionIdentity)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var product = CacheManager.GetProductById(sessionIdentity.ProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, actionInput.RoundId, ProviderId, client.Id, (int)BetDocumentStates.Uncalculated);
            if (betDocument == null) // for test
            {
                var betOperationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = sessionIdentity.Id,
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
                    ExternalProductId = actionInput.GameId,
                    ProductId = product.Id,
                    TransactionId =string.Format("bet_{0}", winAction.ActionId),
                    RoundId = actionInput.RoundId,
                    OperationItems = new List<OperationItemFromProduct>()
                };
                betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                {
                    Client = client,
                    Amount = 0
                });
                betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl/*, out LimitInfo info*/);
                //BaseHelpers.BroadcastBetLimit(info);
            }

            var winDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Win, actionInput.RoundId, ProviderId, client.Id);

            if (winDocument != null)
                BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

            var state = (winAction.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
            betDocument.State = state;

            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = sessionIdentity.Id,
                CurrencyId = client.CurrencyId,
                GameProviderId = ProviderId,
                OperationTypeId = (int)OperationTypes.Win,
                ExternalOperationId = null,
                ExternalProductId = actionInput.GameId,
                ProductId = betDocument.ProductId,
                TransactionId = winAction.ActionId,
                RoundId = actionInput.RoundId,
                CreditTransactionId = betDocument.Id,
                State = state,
                Info = string.Empty,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = winAction.Amount / 100
            });
            var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
            var transaction = new Transaction
            {
                ActionId = winAction.ActionId,
                TxId = doc[0].Id.ToString()
            };
            var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.SoftSwissRollback, winAction.ActionId);
            if (!string.IsNullOrEmpty(rollback))
            {
                var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = winAction.ActionId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id
                };
                try
                {
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                }
                catch {; }
            }
            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            BaseHelpers.BroadcastWin(new ApiWin
            {
                GameName = product.NickName,
                ClientId = client.Id,
                ClientName = client.FirstName,
                Amount = winAction.Amount / 100,
                CurrencyId = client.CurrencyId,
                PartnerId = client.PartnerId,
                ProductId = product.Id,
                ProductName = product.NickName,
                ImageUrl = product.WebImageUrl
            });
            return transaction;
        }

        private static void FinalizeRound(BllClient client, string roundId, int productId, long sessionId)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, roundId, ProviderId,
                                                            client.Id, (int)BetDocumentStates.Uncalculated);
            var listOfOperationsFromApi = new ListOfOperationsFromApi
            {
                SessionId = sessionId,
                CurrencyId = client.CurrencyId,
                RoundId = roundId,
                GameProviderId = ProviderId,
                ProductId = productId,
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
                clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SoftSwiss/rollback")]
        public ActionResult Rollback(BaseInput input)
        {
            var output = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(Request.Body);
                var inputString = bodyStream.ReadToEnd();
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var clientSession = ClientBll.GetClientSessionByProductId(input.ClientId, product.Id);
                var client = CacheManager.GetClientById(input.ClientId);
                var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SoftSwissAuthToken);
                string authHeader = Request.Headers["X-REQUEST-SIGN"];
                var hashString = CommonFunctions.ComputeHMACSha256(inputString, authToken);
                if (hashString.ToLower() != authHeader.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.RoundId = input.RoundId;
                output.Balance = Convert.ToInt32(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
                using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                using var documentBl = new DocumentBll(clientBl);
                var transactions = new List<Transaction>();
                foreach (var ac in input.Actions)
                {
                    if (ac.ActionName == "rollback")
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            GameProviderId = ProviderId,
                            TransactionId = ac.OriginalActionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id
                        };
                        List<DAL.Document> documents = null;
                        try
                        {
                            documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        }
                        catch
                        {
                            CacheManager.SetFutureRollback(Constants.CacheItems.SoftSwissRollback, ac.ActionId, ac.OriginalActionId);
                        }

                        if (documents != null)
                            transactions.Add(new Transaction { ActionId = ac.ActionId, TxId = documents[0].Id.ToString() });
                        else
                            transactions.Add(new Transaction { ActionId = ac.ActionId });

                    }
                }
                output.Balance = Convert.ToInt32(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
                output.RoundId = input.RoundId;
                output.Transactions = transactions;
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                output.Message = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Message = ex.Message;
            }

            return Ok(output);
        }

        [HttpPost]
        [Route("{partnerId}/api/SoftSwiss/freespins")]
        public ActionResult FreeSpin(FreeSpinInput input)
        {
            var output = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(Request.Body);
                var inputString = bodyStream.ReadToEnd();
                var bonusData = input.BonusData.Split('_');
                var client = CacheManager.GetClientById(Convert.ToInt32(bonusData[0]));
                var bonusId = Convert.ToInt32(bonusData[1]);
                var clientBonus = CacheManager.GetClientBonusById(bonusId, client.Id);
                var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SoftSwissAuthToken);
                string authHeader = Request.Headers["X-REQUEST-SIGN"];
                var hashString = CommonFunctions.ComputeHMACSha256(inputString, authToken);
                if (hashString.ToLower() != authHeader.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, 0) * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                output.Message = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Message = ex.Message;
            }
            return Ok(output);
        }
    }
}