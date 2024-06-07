using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.ProductGateway.Models.BlueOcean;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using IqSoft.CP.DAL.Models.Cache;
using System.ServiceModel;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using System.Web.Http.Cors;
using static IqSoft.CP.Common.Constants;
using System.Web;
using IqSoft.CP.Common.Helpers;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Data.Entity.Validation;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class BlueOceanController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BlueOcean).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.BlueOcean);

        private static class ActionTypes
        {
            public const string GetBalance = "balance";
            public const string DoBet = "debit";
            public const string DoWin = "credit";
            public const string Rollback = "rollback";
        }
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Currencies.IranianTuman,
            Constants.Currencies.IranianRial
        };

        [HttpPost]
        [HttpGet]
        [Route("{partnerId}/api/blueOcean")]
        public HttpResponseMessage ApiRequest([FromUri]BaseInput input)
        {
            var output = new BaseOutput { status = (int)HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var queryString = new NameValueCollection(HttpContext.Current.Request.QueryString);
                queryString.Remove("key");
                var inputString = string.Join("&", queryString.AllKeys.Select(x => string.Format("{0}={1}", HttpUtility.UrlEncode(x), HttpUtility.UrlEncode(queryString[x]))));
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(inputString));
                
                var client = CacheManager.GetClientById(input.username);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BlueOceanApiKey);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BlueOceanSalt);
                if (input.key != CommonFunctions.ComputeSha1(salt + inputString))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.callerId != apiKey )
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);

                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    input.amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, input.amount ?? 0);
                int productId = 0;
                if (input.action != ActionTypes.GetBalance)
                {
                    var clientSession = ClientBll.GetClientProductSession(input.session_id + "_" + input.gamesession_id, Constants.DefaultLanguageId, null,
                        (input.action != ActionTypes.DoWin && input.action != ActionTypes.Rollback));

                    switch (input.action)
                    {
                        case ActionTypes.DoBet:
                            output.transaction_id = DoBet(input, clientSession, client, out productId).ToString();
                            break;
                        case ActionTypes.DoWin:
                            output.transaction_id = DoWin(input, clientSession, client, out productId).ToString();
                            break;
                        case ActionTypes.Rollback:
                            output.transaction_id = Rollback(input, clientSession, out productId).ToString();
                            break;
                        default:
                            break;
                    }
                }
                var balance = BaseHelpers.GetClientProductBalance(input.username, productId);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                output.balance = Math.Floor(100 * balance) / 100;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var props = typeof(Constants.Errors).GetFields(BindingFlags.Public | BindingFlags.Static);
                var val = props.FirstOrDefault(prop => (int)prop.GetValue(null) == fex.Detail.Id);
                output.status = val.Name.Contains("NotFound")? (int)HttpStatusCode.NotFound : (int)HttpStatusCode.Forbidden;
                output.msg = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(fex));
            }
            catch (DbEntityValidationException e)
            {
                var m = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    m += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        m += string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                output.status = (int)HttpStatusCode.Forbidden;
                output.msg = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        private long DoBet(BaseInput input, SessionIdentity clientSession, BllClient client, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.game_id);
                    if (product == null || clientSession.ProductId != product.Id)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    productId = product.Id;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(string.Format("{0}_{1}", client.Id, input.transaction_id), client.Id,
                          ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument != null && betDocument.ClientId == client.Id)
                        return betDocument.Id;
                    var amount = Convert.ToDecimal(input.amount);
                    if (input.is_freeround_bet.HasValue && input.is_freeround_bet == 1 )
                        amount = 0;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ExternalProductId = input.game_id,
                        ProductId = clientSession.ProductId,
                        RoundId = input.round_id,
                        TransactionId = string.Format("{0}_{1}", client.Id, input.transaction_id),
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount =  amount,
                        DeviceTypeId = clientSession.DeviceType
                    });
                    betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                    if (input.gameplay_final == 1)
                    {
                        var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.round_id, ProviderId,
                                                                            clientSession.Id, (int)BetDocumentStates.Uncalculated);
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.round_id,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
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
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    BaseHelpers.BroadcastBetLimit(info);
                    return betDocument.Id;
                }
            }
        }

        private long DoWin(BaseInput input, SessionIdentity clientSession, BllClient client, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.game_id);
                    if (product == null || clientSession.ProductId != product.Id)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    productId = product.Id;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.round_id,
                                              ProviderId, client.Id);
                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = input.game_id,
                            ProductId = clientSession.ProductId,
                            RoundId = input.round_id,
                            TransactionId = string.Format("bet{0}_{1}", client.Id, input.transaction_id),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    var winDocument = documentBl.GetDocumentByExternalId(string.Format("{0}_{1}", client.Id, input.transaction_id), client.Id, ProviderId,
                        partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var amount = Convert.ToDecimal(input.amount);
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.round_id,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = input.game_id,
                            ProductId = betDocument.ProductId,
                            TransactionId = string.Format("{0}_{1}", client.Id, input.transaction_id),
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

                        return doc[0].Id;
                    }
                    return 0;
                }
            }
        }

        private long Rollback(BaseInput input, SessionIdentity clientSession, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    productId = product.Id;

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = string.Format("{0}_{1}", clientSession.Id, input.transaction_id),
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId
                    };
                    long resp = 0;
                    try
                    {
                        var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        if (documents == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        resp = documents[0].Id;
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        if (fex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
                            throw;
                        resp = fex.Detail.IntegerInfo ?? 0;
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(input.username);
                    return resp;
                }
            }
        }
    }
}
