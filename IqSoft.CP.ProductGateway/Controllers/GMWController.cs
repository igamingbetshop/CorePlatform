using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.GMW;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class GMWController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.GMW).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.GMW);
        [HttpPost]
        [Route("{partnerId}/api/GMW/Authenticate")]
        public HttpResponseMessage Authenticate(BaseInput input)
        {
            var response = new BaseOutput { Success = true, ErrorCode="0"};
            WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GMWCasinoKey);
                    if (apiKey != input.Key)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    response.PlayerId = client.Id.ToString();
                    response.Currency = client.CurrencyId;
                    response.Balance = Convert.ToInt64(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) * 100);

                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id).ToString();
                response.ErrordDescription = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(Constants.Errors.GeneralException).ToString();
                response.ErrordDescription = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }        

        [HttpPost]
        [Route("{partnerId}/api/GMW/Transaction")]
        public HttpResponseMessage Transaction(TransactionInput input)
        {
            var response = new BaseOutput { Success = true, ErrorCode="0" };

            try
            {
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
                    DoWin( input.RoundId, input.TransactionId, win.Amount / 100, clientSession, client);

                response.PlayerId = client.Id.ToString();
                response.Currency = client.CurrencyId;
                response.Balance =Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) * 100);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id).ToString();
                response.ErrordDescription = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Success = false;
                response.ErrorCode = GMWHelpers.GetErrorCode(Constants.Errors.GeneralException).ToString();
                response.ErrordDescription = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        private void DoBet(string roundId, string TransactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                        clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                }
            }
        }

        private void DoWin(string roundId, string transactionId, decimal amount, SessionIdentity clientSession, BllClient client)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                                BetId = betDocument?.Id ?? 0,
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
    }
}