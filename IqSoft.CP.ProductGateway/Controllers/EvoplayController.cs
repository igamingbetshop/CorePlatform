using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Evoplay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EvoplayController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Evoplay).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Evoplay);

        private static class ActionTypes
        {
            public const string Init = "init";
            public const string DoBet = "bet";
            public const string DoWin = "win";
            public const string Rollback = "refund";
        }

        [HttpPost]
        [Route("{partnerId}/api/evoplay/ApiRequest")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            var output = new BaseOutput { Status = "ok" };
            BllClient client = null;
            try
            {
                WebApiApplication.DbLogger.Info("Input:" + JsonConvert.SerializeObject(input));

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

                output.Data.Balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2);
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(fex));
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
                WebApiApplication.DbLogger.Error(ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        private void DoBet(TransactionDetails input, SessionIdentity clientSession, BllClient client, string callbackId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                        clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                }
            }
        }

        private void DoWin(TransactionDetails input, SessionIdentity clientSession, BllClient client, string callbackId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                                BetAmount = betDocument?.Amount,
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
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
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