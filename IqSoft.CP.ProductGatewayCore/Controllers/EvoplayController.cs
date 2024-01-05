using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Evoplay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class EvoplayController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Evoplay).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "172.255.228.12",
            "172.255.228.228",
            "172.255.228.4",
            "82.115.221.121",
            "82.115.221.122",
            "82.115.221.123",
            "82.115.221.124",
            "82.115.221.125",
            "82.115.221.126",
            "119.42.60.212",
            "119.42.60.220",
            "119.42.60.52",
            "119.42.60.11"
        };

        private static class ActionTypes
        {
            public const string Init = "init";
            public const string DoBet = "bet";
            public const string DoWin = "win";
            public const string Rollback = "refund";
        }

        [HttpPost]
        [Route("{partnerId}/api/evoplay/ApiRequest")]
        [Consumes("application/x-www-form-urlencoded")]

        public ActionResult  ApiRequest([FromForm]BaseInput input)
        {
            var output = new BaseOutput { Status = "ok" };
            BllClient client = null;
            try
            {
                Program.DbLogger.Info("Input:" + JsonConvert.SerializeObject(input));

                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId, null,
                                                                     (input.name == ActionTypes.Init || input.name == ActionTypes.DoBet));
                client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvoplayApiKey);
                var projectId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvoplayProjectId);
                var inputSign = input.signature;
                input.signature = null;
                if (input.name == ActionTypes.DoWin)
                {
                    input.data.win_action_id = input.data.action_id;
                    input.data.action_id = null;
                    input.data.win_final_action = input.data.final_action;
                    input.data.final_action = null;
                }
                var sign = string.Format("{0}*1*{1}*{2}", projectId, Integration.Products.Helpers.EvoplayHelpers.GetSignatureString(input), apiKey);
                if (CommonFunctions.ComputeMd5(sign) != inputSign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.Data = new Details
                {
                    Currency = client.CurrencyId
                };
                switch (input.name)
                {
                    case ActionTypes.DoBet:
                        DoBet(input.data, clientSession, client, input.callback_id);
                        break;
                    case ActionTypes.DoWin:
                        DoWin(input.data, clientSession, client, input.callback_id);
                        break;
                    case ActionTypes.Rollback:
                        Rollback((input.data).refund_callback_id, clientSession);
                        break;
                    default:
                        break;
                }

                output.Data.Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Status = "error";
                output.Error = new ErrorDetails
                {
                    NoRefund = "0",
                    Scope = "user"
                };
                if (fex.Detail.Id == Constants.Errors.LowBalance)
                    output.Error.Message = "Not enough money";
                else
                    output.Error.Message = fex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(fex));
            }
            catch (Exception ex)
            {
                output.Status = "error";
                output.Error = new ErrorDetails
                {
                    Message = ex.Message,
                    NoRefund = "0",
                    Scope = "user"
                };
                Program.DbLogger.Error(ex);
            }
            return new JsonResult(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static void DoBet(TransactionDetails input, SessionIdentity clientSession, BllClient client, string callbackId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(callbackId, client.Id, ProviderId,
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
                            RoundId = input.action_id,
                            TransactionId = callbackId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = Convert.ToDecimal(input.amount),
                            DeviceTypeId = clientSession.DeviceType
                        });
                        clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                }
            }
        }

        private static void DoWin(TransactionDetails input, SessionIdentity clientSession, BllClient client, string callbackId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.win_action_id, ProviderId, client.Id);
                    if (betDocument != null)
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(callbackId, client.Id, ProviderId, partnerProductSetting.Id,
                                                                            (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var amount = Convert.ToDecimal(input.amount);
                            var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.win_action_id,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ExternalProductId = product.ExternalId,
                                ProductId = betDocument.ProductId,
                                TransactionId = callbackId,
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

        private void Rollback(string betId, SessionIdentity clientSession)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductById(clientSession.ProductId);

                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = clientSession.SessionId,
                    GameProviderId = ProviderId,
                    TransactionId = betId,
                    ExternalProductId = product.ExternalId,
                    ProductId = clientSession.ProductId
                };
                try
                {
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                }
                catch
                { }

                BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
            }
        }
    }
}