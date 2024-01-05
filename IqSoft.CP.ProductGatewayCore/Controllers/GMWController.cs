using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.GMW;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class GMWController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.GMW).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            ""
        };

        [HttpPost]
        [Route("{partnerId}/api/GMW/Authenticate")]
        public ActionResult Authenticate(BaseInput input)
        {
            var response = new BaseOutput { Success = true, ErrorCode = "0" };
            Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GMWCasinoKey);
                    if (apiKey != input.Key)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    response.PlayerId = client.Id.ToString();
                    response.Currency = client.CurrencyId;
                    response.Balance = Convert.ToInt64(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100);

                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id).ToString();
                response.ErrordDescription = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(Constants.Errors.GeneralException).ToString();
                response.ErrordDescription = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/GMW/Transaction")]
        public ActionResult Transaction(TransactionInput input)
        {
            var response = new BaseOutput { Success = true, ErrorCode = "0" };

            try
            {
                Program.DbLogger.Info("Input:" + JsonConvert.SerializeObject(input));

                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null,
                    input.Transactions.Any(x => x.Type == GMWHelpers.TransactionTypes.Bet));
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GMWCasinoKey);
                if (apiKey != input.Key)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var bet = input.Transactions.FirstOrDefault(x => x.Type == GMWHelpers.TransactionTypes.Bet);
                if (bet != null)
                    DoBet(input.RoundId, input.TransactionId, bet.Amount / 100, clientSession, client);

                var win = input.Transactions.FirstOrDefault(x => x.Type == GMWHelpers.TransactionTypes.Win);
                if (win != null)
                    DoWin(input.RoundId, input.TransactionId, win.Amount / 100, clientSession, client);

                response.PlayerId = client.Id.ToString();
                response.Currency = client.CurrencyId;
                response.Balance = Convert.ToInt32(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id).ToString();
                response.ErrordDescription = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(Constants.Errors.GeneralException).ToString();
                response.ErrordDescription = ex.Message;
            }
            return Ok(response);
        }

        private static void DoBet(string roundId, string TransactionId, decimal amount, SessionIdentity clientSession, BllClient client)
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
                        clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                }
            }
        }

        private static void DoWin(string roundId, string transactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, roundId, partnerProductSetting.ProviderId,
                                                                       client.Id, (int)BetDocumentStates.Uncalculated);

                    if (betDocument != null)
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
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
                    }
                }
            }
        }
    }
}