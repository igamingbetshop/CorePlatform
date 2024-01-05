using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Habanero;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class HabaneroController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Habanero).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "52.184.81.237"
        };

        [HttpPost]
        [Route("{partnerId}/api/Habanero/ApiRequest")]
        public ActionResult ApiRequest(CommonInput input)
        {
            var output = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var client = new BllClient();
                switch (input.Action)
                {
                    case HabaneroHelpers.Actions.Authenticate:
                        var clientSession = ClientBll.GetClientProductSession(input.PlayerDetails.Token, Constants.DefaultLanguageId, null, true);
                        client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
                        output.PlayerDetails = new PlayerDetailOutput
                        {
                            StatusDetails = new StatusOutput
                            {
                                Success = true,
                                Autherror = false,
                                Message = string.Empty
                            },
                            AccountId = client.Id.ToString(),
                            AccountName = client.UserName,
                            Balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                            CurrencyCode = client.CurrencyId
                        };
                        break;
                    case HabaneroHelpers.Actions.FundTransfer:
                        if (input.TransferDetails.Funds.FundInfoDetails != null &&
                            input.TransferDetails.Funds.FundInfoDetails.Any() && input.TransferDetails.Funds.RefundDetails == null)
                        {
                            if (!input.TransferDetails.Funds.DebitAndCredit && !string.IsNullOrEmpty(input.TransferDetails.Funds.TransferId))
                            {
                                input.TransferDetails.Funds.FundInfoDetails = new List<FundInfo>
                                {
                                   new FundInfo
                                   {

                                       TransferId = input.TransferDetails.Funds.TransferId,
                                       GameStatemode = input.TransferDetails.Funds.GameStatemode,
                                       Amount = input.TransferDetails.Funds.Amount,
                                       InitialDebitTransferId = input.TransferDetails.Funds.InitialDebitTransferId,
                                   }
                                };
                            }
                            client = DoBetWin(input.TransferDetails, input.GameDetails);
                            output.FundTransferDetails = new FundTransferOutput
                            {
                                StatusDetails = new StatusOutput
                                {
                                    Success = true,
                                    Autherror = false,
                                    Message = string.Empty,
                                    NoFunds = false
                                },
                                AccountId = client.Id.ToString(),
                                AccountName = client.UserName,
                                Balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                                CurrencyCode = client.CurrencyId
                            };
                            if (input.TransferDetails.Funds.FundInfoDetails.Any(x => x.GameStatemode == 1))
                                output.FundTransferDetails.StatusDetails.SuccessDebit = true;
                            if (input.TransferDetails.Funds.FundInfoDetails.Any(x => x.GameStatemode == 2))
                                output.FundTransferDetails.StatusDetails.SuccessCredit = true;
                        }
                        else if (input.TransferDetails.Funds.RefundDetails != null)
                        {
                            client = Rollback(input.TransferDetails.Funds.RefundDetails, Convert.ToInt32(input.TransferDetails.ClientId), input.GameDetails);
                            output.FundTransferDetails = new FundTransferOutput
                            {
                                StatusDetails = new StatusOutput
                                {
                                    Success = true,
                                    Autherror = false,
                                    Message = string.Empty,
                                    NoFunds = false,
                                    RefundStatus = 1
                                },
                                AccountId = client.Id.ToString(),
                                AccountName = client.UserName,
                                Balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                                CurrencyCode = client.CurrencyId
                            };
                        }
                        break;
                    case HabaneroHelpers.Actions.QueryRequest:
                        CheckTransfer(input.QueryDetails);
                        output.FundTransferDetails = new FundTransferOutput
                        {
                            StatusDetails = new StatusOutput
                            {
                                Success = true
                            }
                        };
                        break;
                    case HabaneroHelpers.Actions.AltFundsRequest:
                        client = TournamentCredit(input.AltFundsDetails);
                        output.FundTransferDetails = new FundTransferOutput
                        {
                            StatusDetails = new StatusOutput
                            {
                                Success = true
                            },
                            Balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                            CurrencyCode = client.CurrencyId
                        };
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? faultException.Message : faultException.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "Error: " + errorMessage);
                var statusDetails = new StatusOutput
                {
                    Success = false,
                    Autherror = true,
                    Message = errorMessage
                };
                switch (input.Action)
                {
                    case HabaneroHelpers.Actions.Authenticate:
                        output.PlayerDetails = new PlayerDetailOutput
                        {
                            StatusDetails = statusDetails
                        };
                        break;

                    case HabaneroHelpers.Actions.FundTransfer:
                        output.FundTransferDetails = new FundTransferOutput
                        {
                            StatusDetails = statusDetails
                        };
                        if (input.TransferDetails.Funds.FundInfoDetails != null &&
                            input.TransferDetails.Funds.FundInfoDetails.Any())
                        {
                            if (input.TransferDetails.Funds.FundInfoDetails.Any(x => x.GameStatemode == 1))
                                output.FundTransferDetails.StatusDetails.SuccessDebit = false;
                            if (input.TransferDetails.Funds.FundInfoDetails.Any(x => x.GameStatemode == 2))
                                output.FundTransferDetails.StatusDetails.SuccessCredit = false;
                            if (faultException.Detail != null && faultException.Detail.Id == Constants.Errors.LowBalance)
                                output.FundTransferDetails.StatusDetails.NoFunds = true;
                        }
                        else if (input.TransferDetails.Funds.RefundDetails != null)
                        {
                            output.FundTransferDetails.StatusDetails.RefundStatus = 2;
                            output.FundTransferDetails.StatusDetails.Message = string.Empty;
                        }
                        break;
                    case HabaneroHelpers.Actions.QueryRequest:
                    case HabaneroHelpers.Actions.AltFundsRequest:
                        output.FundTransferDetails = new FundTransferOutput
                        {
                            StatusDetails = statusDetails
                        };
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "Error: " + ex);
                var statusDetails = new StatusOutput
                {
                    Success = false,
                    Autherror = true,
                    Message = errorMessage
                };
                switch (input.Action)
                {
                    case HabaneroHelpers.Actions.Authenticate:
                        output.PlayerDetails = new PlayerDetailOutput
                        {
                            StatusDetails = statusDetails
                        };
                        break;

                    case HabaneroHelpers.Actions.FundTransfer:
                        output.FundTransferDetails = new FundTransferOutput
                        {
                            StatusDetails = statusDetails
                        };
                        if (input.TransferDetails.Funds.FundInfoDetails != null &&
                            input.TransferDetails.Funds.FundInfoDetails.Any() && input.TransferDetails.Funds.DebitAndCredit)
                        {
                            if (input.TransferDetails.Funds.FundInfoDetails.Any(x => x.GameStatemode == 1))
                                output.FundTransferDetails.StatusDetails.SuccessDebit = false;
                            if (input.TransferDetails.Funds.FundInfoDetails.Any(x => x.GameStatemode == 2))
                                output.FundTransferDetails.StatusDetails.SuccessCredit = false;
                        }
                        else if (input.TransferDetails.Funds.RefundDetails != null)
                        {
                            output.FundTransferDetails.StatusDetails.RefundStatus = 2;
                            output.FundTransferDetails.StatusDetails.Message = string.Empty;
                        }
                        break;
                    case HabaneroHelpers.Actions.QueryRequest:
                        output.FundTransferDetails = new FundTransferOutput
                        {
                            StatusDetails = statusDetails
                        };
                        break;
                    default:
                        break;
                }
            }
            return Ok(output);
        }

        private static void CheckTransfer(QueryInput input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var document = documentBl.GetDocumentOnlyByExternalId(input.TransferId, ProviderId, 
                   Convert.ToInt32( input.AccountId), (input.QueryAmount < 0 ? (int)OperationTypes.Bet : (int)OperationTypes.Win));
                if (document == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
            }
        }

        private static BllClient DoBetWin(TransferInput input, Game gameDetails)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
            //    var product = CacheManager.GetProductById(clientSession.ProductId);
            var product = CacheManager.GetProductByExternalId(ProviderId, gameDetails.KeyName);

            using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                    var betTransfer = input.Funds.FundInfoDetails.FirstOrDefault(x => x.GameStatemode == 1 || (x.GameStatemode == 0 && x.Amount < 0));
                    if (betTransfer != null)
                    {
                        var document = documentBl.GetDocumentByExternalId(betTransfer.TransferId, clientSession.Id, ProviderId,
                                                                           partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = betTransfer.TransferId,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = betTransfer.TransferId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = Math.Abs(betTransfer.Amount),
                                DeviceTypeId = clientSession.DeviceType
                            });
                            document = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);
                            if (betTransfer.GameStatemode == 0)
                            {
                                var winListOfOperationsFromApi = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    State = (int)BetDocumentStates.Lost,
                                    CurrencyId = client.CurrencyId,
                                    RoundId = betTransfer.TransferId,
                                    GameProviderId = ProviderId,
                                    ExternalProductId = product.ExternalId,
                                    ProductId = product.Id,
                                    TransactionId = betTransfer.TransferId,
                                    CreditTransactionId = document.Id,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                winListOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0
                                });
                                clientBl.CreateDebitsToClients(winListOfOperationsFromApi, document, documentBl);
                            }
                        }
                    }
                    var winTransfer = input.Funds.FundInfoDetails.FirstOrDefault(x => x.GameStatemode == 2 || (x.GameStatemode == 0 && x.Amount >= 0));
                    if (winTransfer != null)
                    {
                        if (winTransfer.IsBonus || winTransfer.JpWin)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = winTransfer.TransferId,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = string.Format("bonus_{0}", winTransfer.TransferId),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);
                            winTransfer.InitialDebitTransferId = listOfOperationsFromApi.TransactionId;
                        }
                        var betDocument = documentBl.GetDocumentByExternalId(winTransfer.InitialDebitTransferId, clientSession.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument != null)
                        {
                            var winDocument = documentBl.GetDocumentByExternalId(winTransfer.TransferId, clientSession.Id, ProviderId,
                                                                              partnerProductSetting.Id, (int)OperationTypes.Win);
                            if (winDocument == null)
                            {
                                var listOfOperationsFromApi = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    State = (winTransfer.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost),
                                    CurrencyId = client.CurrencyId,
                                    RoundId = winTransfer.TransferId,
                                    GameProviderId = ProviderId,
                                    ExternalProductId = product.ExternalId,
                                    ProductId = product.Id,
                                    TransactionId = winTransfer.TransferId,
                                    CreditTransactionId = betDocument.Id,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = winTransfer.Amount
                                });
                                clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDocument, documentBl);
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    Amount = winTransfer.Amount,
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
            return client;
        }

        private static BllClient Rollback(Refund input, int clientId, Game gameDetails)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        TransactionId = input.OriginalTransferId,
                        ExternalProductId = gameDetails.KeyName
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(clientId);
                    return client;
                }
            }
        }

        private static BllClient TournamentCredit(AltFundsInput input)
        {
            var client = CacheManager.GetClientById(Convert.ToInt32(input.AccountId));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

            var product = CacheManager.GetProductByExternalId(ProviderId, "TournamentWin");
            if (product == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
            if (partnerProductSetting == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var winDocument = documentBl.GetDocumentByExternalId(input.TransferId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            RoundId = input.TransferId,
                            ExternalProductId = product.ExternalId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = string.Format("Tournament_{0}", input.TransferId),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var betDocument = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);

                        var winListOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            State = (int)BetDocumentStates.Won,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.TransferId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = input.TransferId,
                            CreditTransactionId = betDocument.Id,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        winListOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount
                        });
                        clientBl.CreateDebitsToClients(winListOfOperationsFromApi, betDocument, documentBl);
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
                    }
                }
            }
            return client;
        }
    }
}