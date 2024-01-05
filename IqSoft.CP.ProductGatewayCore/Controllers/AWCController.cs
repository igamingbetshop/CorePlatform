using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.AWC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Transaction = IqSoft.CP.ProductGateway.Models.AWC.Transaction;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class AWCController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.AWC).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "1.32.212.184",
            "1.32.212.188",
            "1.32.212.194",
            "1.32.212.195",
            "1.32.212.196",
            "1.32.212.197",
            "1.32.212.198",
            "1.32.212.199",
            "218.189.17.200",
            "218.189.17.21*",
            "218.189.17.220",
            "218.189.17.221",
            "218.189.17.222",
            "218.189.17.18",
            "218.189.17.19",
            "218.189.17.2*",
            "218.189.17.30",
            "1.32.212.2**",
            "1.32.212.20",
            "118.163.33.139",
            "1.32.212.187",
            "54.150.100.111",
            "59.120.133.194",
            "123.51.185.146",
            "1.32.212.189",
            "1.32.210.126",
            "103.4.104.63",
            "202.153.89.250",
            "205.252.17.83",
            "218.32.214.9",
        };
        public static List<string> Currenies = new List<string>
        {
            Constants.Currencies.CambodianRiel
        };


        [HttpPost]
        [Route("{partnerId}/api/AWC/ApiRequest")]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult ApiRequest([FromForm] BaseInput input)
        {
            var message = JsonConvert.DeserializeObject<MessageModel>(input.message);
            var output = new BaseOutput
            {
                Status = "0000",
                UserId = message.UserId,
                Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK")
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var clientId = 0;
                if (message.Action == AWCHelpers.Methods.GetBalance)
                {
                    if (string.IsNullOrEmpty(message.UserId) || !Int32.TryParse(message.UserId, out clientId))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                }
                else if (message.Transactions == null || message.Transactions.Count == 0 || !Int32.TryParse(message.Transactions[0].UserId, out clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                output.UserId = clientId.ToString();
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.AWCSecureKey);
                if (apiKey != input.key)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
                switch (message.Action)
                {
                    case AWCHelpers.Methods.GetBalance:
                         ClientBll.GetClientProductSession(input.extension1, Constants.DefaultLanguageId);
                        break;
                    case AWCHelpers.Methods.DoBet:
                        foreach (var tnx in message.Transactions)
                            DoBet(tnx, input.extension1);
                        break;
                    case AWCHelpers.Methods.AdjustBet:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                tnx.WinAmount = tnx.AdjustAmount;
                                DoWin(tnx, currClientId, input.extension1, string.Format("{0}_{0}", tnx.PlatformTxId));
                            }
                        }
                        break;
                    case AWCHelpers.Methods.Rollback:
                    case AWCHelpers.Methods.VoidBet:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                                BetRollback(tnx, currClientId);
                        }
                        break;
                    case AWCHelpers.Methods.RollbackBetNSettle:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                WinRollback(tnx, currClientId);
                                BetRollback(tnx, currClientId);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.Resettle:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                WinRollback(tnx, currClientId);
                                DoWin(tnx, currClientId, input.extension1, "ressetle_" + tnx.PlatformTxId);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.DoWin:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                DoWin(tnx, currClientId, input.extension1);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.BetNSettle:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                DoBet(tnx, input.extension1);
                                DoWin(tnx, currClientId, input.extension1);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.DoLost:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                WinRollback(tnx, currClientId);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.VoidSettle:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                VoidSettle(tnx, currClientId);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.BonusWin:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                DoBonusWin(tnx, currClientId, input.extension1);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.DonateTip:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                DonateTip(tnx, currClientId, input.extension1);
                            }
                        }
                        break;
                    case AWCHelpers.Methods.CancelTip:
                        foreach (var tnx in message.Transactions)
                        {
                            if (Int32.TryParse(tnx.UserId, out int currClientId))
                            {
                                tnx.PlatformTxId = string.Format("tipBet_{0}", tnx.PlatformTxId);
                                BetRollback(tnx, currClientId);
                            }
                        }
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                }
                output.Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientId).AvailableBalance, 2);
                if (Currenies.Contains(client.CurrencyId))
                    output.Balance *= 1000;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = AWCHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);

                output.Status = AWCHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + fex.Detail.Id);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex);
                output.Status = AWCHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }

            return Ok(JsonConvert.SerializeObject(output));
        }

        private static void DoBet(Transaction input, string sessionToken)
        {
            var clientSession = ClientBll.GetClientProductSession(sessionToken, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.UserId));
                    var product = CacheManager.GetProductByExternalId(ProviderId, string.Format("{0}|{1}|{2}", input.GameCode, input.GameType, input.Platform));

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var document = documentBl.GetDocumentByExternalId(input.PlatformTxId, client.Id, ProviderId,
                                                                            partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        if (Currenies.Contains(client.CurrencyId))
                            input.Amount /= 1000;
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                          //  SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            ExternalProductId = product.ExternalId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = input.PlatformTxId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);
                        var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.AWCRollback, input.PlatformTxId);
                        if (!string.IsNullOrEmpty(rollback))
                        {
                            var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                            {
                                GameProviderId = ProviderId,
                                TransactionId = input.PlatformTxId,
                                ExternalProductId = product.ExternalId,
                                ProductId = product.Id
                            };
                            try
                            {
                                documentBl.RollbackProductTransactions(rollbackOperationsFromProduct);
                            }
                            catch {; }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                }
            }
        }

        private static void DoWin(Transaction input, int clientId, string sessionToken, string newTransactionId = "")
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    //var clientSession = ClientBll.GetClientProductSession(sessionToken, Constants.DefaultLanguageId, null, false);
                    var client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    var product = CacheManager.GetProductByExternalId(ProviderId, string.Format("{0}|{1}|{2}", input.GameCode, input.GameType, input.Platform));
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var betDocument = documentBl.GetDocumentByExternalId(input.PlatformTxId, client.Id, ProviderId,
                                                                            partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (betDocument != null)
                    {
                        if (Currenies.Contains(client.CurrencyId))
                            input.WinAmount /= 1000;

                        var state = input.WinAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        newTransactionId = string.Format("win_{0}", string.IsNullOrEmpty(newTransactionId) ? input.PlatformTxId : newTransactionId);
                        var winDocument = documentBl.GetDocumentByExternalId(newTransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                //SessionId = clientSession.SessionId,
                                State = state,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                ExternalProductId = product.ExternalId,
                                ProductId = product.Id,
                                TransactionId = newTransactionId,
                                CreditTransactionId = betDocument.Id,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.WinAmount.Value
                            });
                            clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDocument, documentBl);
                            var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.AWCRollback, newTransactionId);
                            if (!string.IsNullOrEmpty(rollback))
                            {
                                var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                                {
                                    GameProviderId = ProviderId,
                                    TransactionId = newTransactionId,
                                    ExternalProductId = product.ExternalId,
                                    ProductId = product.Id
                                };
                                try
                                {
                                    documentBl.RollbackProductTransactions(rollbackOperationsFromProduct);
                                }
                                catch {; }
                            }

                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = input.WinAmount.Value,
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

        private static void BetRollback(Transaction input, int clientId)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductByExternalId(ProviderId, string.Format("{0}|{1}|{2}", input.GameCode, input.GameType, input.Platform));
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = input.PlatformTxId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id,
                    OperationTypeId = (int)OperationTypes.Bet
                };
                try
                {
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                }
                catch
                {
                    CacheManager.SetFutureRollback(Constants.CacheItems.AWCRollback, input.PlatformTxId, input.PlatformTxId);
                }

                BaseHelpers.RemoveClientBalanceFromeCache(clientId);
            }
        }

        private static void WinRollback(Transaction input, int clientId)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductByExternalId(ProviderId, string.Format("{0}|{1}|{2}", input.GameCode, input.GameType, input.Platform));
                var transactionId = string.Format("win_{0}", input.PlatformTxId);
                var winOperationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = transactionId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id,
                    OperationTypeId = (int)OperationTypes.Win
                };
                try
                {
                    documentBl.RollbackProductTransactions(winOperationsFromProduct, newExternalId: string.Format("win_{0}_{1}", input.PlatformTxId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                    BaseHelpers.RemoveClientBalanceFromeCache(clientId);
                }
                catch
                {
                    CacheManager.SetFutureRollback(Constants.CacheItems.AWCRollback, transactionId, transactionId);
                }
            }
        }

        private static void VoidSettle(Transaction input, int clientId)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductByExternalId(ProviderId, string.Format("{0}|{1}|{2}", input.GameCode, input.GameType, input.Platform));
                var winOperationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = string.Format("win_{0}", input.PlatformTxId),
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id
                };
                try
                {
                    documentBl.RollbackProductTransactions(winOperationsFromProduct, newExternalId: string.Format("win_{0}_{1}", input.PlatformTxId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                }
                catch {; }
                var betOperationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = input.PlatformTxId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id
                };
                try
                {
                    documentBl.RollbackProductTransactions(betOperationsFromProduct);
                }
                catch {; }
                BaseHelpers.RemoveClientBalanceFromeCache(clientId);
            }
        }
        private static void DoBonusWin(Transaction input, int clientId, string sessionToken)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(sessionToken, Constants.DefaultLanguageId, null, false);
                    var client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var externalBonusId = string.Format("bonusWin_{0}_{1}", input.PromotionId, input.PromotionTxId);
                    var winDocument = documentBl.GetDocumentByExternalId(externalBonusId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var betOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            //SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = string.Format("bonusBet_{0}_{1}", input.PromotionId, input.PromotionTxId),
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl);
                        if (Currenies.Contains(client.CurrencyId))
                            input.BonusAmount /= 1000;

                        var winOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            //SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.PromotionTxId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            TransactionId = externalBonusId,
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        winOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.BonusAmount
                        });

                        winDocument = clientBl.CreateDebitsToClients(winOperationsFromProduct, betDocument, documentBl)[0];
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = input.BonusAmount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                }
            }
        }

        private static void DonateTip(Transaction input, int clientId, string sessionToken)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(sessionToken, Constants.DefaultLanguageId, null, false);
                    var client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductByExternalId(ProviderId, string.Format("{0}|{1}|{2}", input.GameCode, input.GameType, input.Platform));
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var externalTipId = string.Format("tipBet_{0}", input.PlatformTxId);

                    var betDocument = documentBl.GetDocumentByExternalId(externalTipId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (betDocument == null)
                    {
                        if (Currenies.Contains(client.CurrencyId))
                            input.TipAmount /= 1000;
                        var betOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = externalTipId,
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.TipAmount
                        });
                        betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl);
                        var winOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            RoundId = input.PromotionTxId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            TransactionId = string.Format("tipWin_{0}", input.PlatformTxId),
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        winOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        clientBl.CreateDebitsToClients(winOperationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                }
            }
        } 
    }
}