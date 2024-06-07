using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
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
using IqSoft.CP.ProductGateway.Models.ISoftBet;
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class ISoftBetController : ApiController
    {
        private readonly int Rate = 100;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.ISoftBet);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.ISoftBet).Id;
        [HttpPost]
        [Route("{partnerId}/api/ISoftBet/ApiRequest")]
        public HttpResponseMessage ApiRequest(int partnerId, BaseInput input, [FromUri]string hash)
        {
            var jsonResponse = string.Empty;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.ISoftBetSecretKey);
                if (string.IsNullOrEmpty(secretKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                var jsonMessage = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                if (hash.ToLower() != Common.Helpers.CommonFunctions.ComputeHMACSha256(jsonMessage, secretKey).ToLower())
                {
                    WebApiApplication.DbLogger.Error(Constants.Errors.WrongHash + "_" + hash + "_" + jsonMessage);
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                }
                if (string.IsNullOrEmpty(input.Multiplier) || input.Multiplier == "0")
                    input.Multiplier = "1";
                if (input.State.ToLower() == "multi")
                {
                    foreach (var action in input.Actions)
                    {
                        var parameters = action.Parameters == null ? null : JsonConvert.DeserializeObject<Parameter>(action.Parameters.ToString());
                        switch (action.Command)
                        {
                            case "init":
                                jsonResponse = JsonConvert.SerializeObject(PlayerAuthorization(parameters, input.SessionId,
                                    Convert.ToDecimal(input.Multiplier)));
                                break;
                            case "balance":
                                jsonResponse = JsonConvert.SerializeObject(GetBalance(Convert.ToInt32(input.PlayerId),
                                    Convert.ToDecimal(input.Multiplier)));
                                break;
                            case "bet":
                                jsonResponse = JsonConvert.SerializeObject(DoBet(input, partnerId, parameters));
                                break;
                            case "win":
                                jsonResponse = JsonConvert.SerializeObject(DoWin(input, partnerId, parameters));
                                break;
                            case "cancel":
                                jsonResponse = JsonConvert.SerializeObject(Cancel(input, partnerId, parameters));
                                break;
                            case "end":
                                jsonResponse = JsonConvert.SerializeObject(EndOfSession(input, partnerId));
                                break;
                            default:
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                        }
                    }
                }
                else
                {
                    var parameters = input.Action.Parameters == null ? null : JsonConvert.DeserializeObject<Parameter>(input.Action.Parameters.ToString());
                    switch (input.Action.Command)
                    {
                        case "init":
                            jsonResponse = JsonConvert.SerializeObject(PlayerAuthorization(parameters, input.SessionId,
                                Convert.ToDecimal(input.Multiplier)));
                            break;
                        case "balance":
                            jsonResponse = JsonConvert.SerializeObject(GetBalance(Convert.ToInt32(input.PlayerId),
                                Convert.ToDecimal(input.Multiplier)));
                            break;
                        case "bet":
                            jsonResponse = JsonConvert.SerializeObject(DoBet(input, partnerId, parameters));
                            break;
                        case "win":
                            jsonResponse = JsonConvert.SerializeObject(DoWin(input, partnerId, parameters));
                            break;
                        case "cancel":
                            jsonResponse = JsonConvert.SerializeObject(Cancel(input, partnerId, parameters));
                            break;
                        case "end":
                            jsonResponse = JsonConvert.SerializeObject(EndOfSession(input, partnerId));
                            break;
                        case "token":
                            jsonResponse = JsonConvert.SerializeObject(GetNewToken(input));
                            break;
                        default:
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(fex);
                var errorResponse = new ErrorResponse
                {
                    Status = ISoftBetHelpers.Statuses.Error,
                    ErrorCode = ISoftBetHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message,
                    Display = "true",
                    Action = "void"
                };
                jsonResponse = JsonConvert.SerializeObject(errorResponse);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var errorResponse = new ErrorResponse
                {
                    Status = ISoftBetHelpers.Statuses.Error,
                    ErrorCode = ISoftBetHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message,
                    Display = "true",
                    Action = "void"
                };
                jsonResponse = JsonConvert.SerializeObject(errorResponse);
            }
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8)
            };
            return httpResponse;
        }

        private InitOutput PlayerAuthorization(Parameter input, string productSessionId, decimal multiplier)
        {
            using (var clientBll = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                clientSession.Token = productSessionId;
                ClientBll.CreateNewProductSession(clientSession, out List<BllClientSession> oldSessions, token: clientSession.Token);
                foreach (var s in oldSessions)
                    BaseHelpers.RemoveSessionFromeCache(s.Token, s.ProductId);
                var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                return new InitOutput
                {
                    Status = ISoftBetHelpers.Statuses.Success,
                    SessionId = clientSession.Token,
                    PlayerId = client.Id.ToString(),
                    CurrencId = client.CurrencyId,
                    Balance = Convert.ToInt64((BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) / multiplier) * Rate)
                };
            }
        }

        private TransactionOutput GetBalance(int playerId, decimal multiplier)
        {
            var client = CacheManager.GetClientById(playerId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            return new TransactionOutput
            {
                Status = ISoftBetHelpers.Statuses.Success,
                Currency = client.CurrencyId,
                Balance = Convert.ToInt64((BaseHelpers.GetClientProductBalance(client.Id, 0) / multiplier) * Rate)
            };
        }

        private TransactionOutput EndOfSession(BaseInput input, int partnerId)
        {
            var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
            var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);


            return new TransactionOutput
            {
                Status = ISoftBetHelpers.Statuses.Success,
                Currency = client.CurrencyId,
                Balance = Convert.ToInt64((BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) / Convert.ToDecimal(input.Multiplier)) * Rate)
            };
        }

        private TransactionOutput DoBet(BaseInput input, int partnerId, Parameter p)
        {
            var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                    clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var document = documentBl.GetDocumentByExternalId(p.TransactionId, client.Id,
                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = p.RoundId,
                            GameProviderId = ProviderId,
                            ExternalProductId = input.GameId,
                            ProductId = clientSession.ProductId,
                            TransactionId = p.TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = (p.Amount.Value * Convert.ToDecimal(input.Multiplier)) / Rate,
                            DeviceTypeId = clientSession.DeviceType
                        });
						clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    return new TransactionOutput
                    {
                        Status = ISoftBetHelpers.Statuses.Success,
                        Currency = client.CurrencyId,
                        Balance = Convert.ToInt64((BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) / Convert.ToDecimal(input.Multiplier)) * Rate)
                    };
                }
            }
        }

        private TransactionOutput DoWin(BaseInput input, int partnerId, Parameter p)
        {
            var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                    clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, p.RoundId, ProviderId, client.Id);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                    var winDocument = documentBl.GetDocumentByExternalId(p.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        var state = (p.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = p.RoundId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = input.GameId,
                            ProductId = betDocument.ProductId,
                            TransactionId = p.TransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        decimal jcp = 0;
                        if (p.WinJackpotContribution.HasValue)
                            jcp = (p.WinJackpotContribution.Value * Convert.ToDecimal(input.Multiplier)) / Rate;
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = ((p.Amount.Value * Convert.ToDecimal(input.Multiplier)) / Rate) + jcp
                        });

						clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            BetAmount = betDocument?.Amount,
                            Amount = operationsFromProduct.OperationItems[0].Amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = partnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    return new TransactionOutput
                    {
                        Status = ISoftBetHelpers.Statuses.Success,
                        Currency = client.CurrencyId,
                        Balance = Convert.ToInt64((BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) / Convert.ToDecimal(input.Multiplier)) * Rate)
                    };
                }
            }
        }

        private TransactionOutput Cancel(BaseInput input, int partnerId, Parameter p)
        {
            var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                    clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = clientSession.SessionId,
                    GameProviderId = ProviderId,
                    TransactionId = p.TransactionId,
                    ProductId = clientSession.ProductId
                };
                var betDocument = documentBl.GetDocumentByExternalId(p.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                if (betDocument == null)
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                }
                if (betDocument.State != (int)BetDocumentStates.Deleted)
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                return new TransactionOutput
                {
                    Status = ISoftBetHelpers.Statuses.Success,
                    Currency = client.CurrencyId,
                    Balance = Convert.ToInt64((BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) / Convert.ToDecimal(input.Multiplier)) * Rate)
                };
            }
        }

        private TokenOutput GetNewToken(BaseInput input)
        {
            using (var clientBll = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                var session = new SessionIdentity
                {
                    Id = client.Id,
                    LoginIp = string.Empty,
                    LanguageId = Constants.DefaultLanguageId,
                    ProductId = product.Id
                };
                var newSession = clientBll.RefreshClientSession(session.Token, true).Token;
                BaseHelpers.RemoveSessionFromeCache(session.Token, session.ProductId);
                return new TokenOutput
                {
                    Status = ISoftBetHelpers.Statuses.Success,
                    Token = newSession
                };
            }
        }
	}
}