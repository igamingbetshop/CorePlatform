using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.AleaPlay;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using IqSoft.CP.DAL;
using System.Net.Http.Headers;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models.Clients;
using IntgrationAleaHelpers = IqSoft.CP.Integration.Products.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class AleaPlayController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.AleaPlay).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.AleaPlay);

        [HttpGet]
        [Route("{partnerId}/api/AleaPlay/players/{casinoPlayerId}/balance")]
        public HttpResponseMessage GetBalance([FromUri] BaseInput input, string casinoPlayerId)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Digest"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("Digest").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (!int.TryParse(casinoPlayerId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.AleaPlaySecretKey);
                var sign = $"{input.casinoSessionId}{input.currency}{input.gameId}" +
                                                         $"{input.integratorId}{input.softwareId}{secretKey}";
                sign = CommonFunctions.ComputeSha512(sign);
                if (sign.ToLower() != signature.Replace("SHA-512=", string.Empty).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientProductSession(input.casinoSessionId, Constants.DefaultLanguageId);
                if (clientSession.Id != clientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.ExternalId != input.gameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var subProviderName = string.Empty;
                if (product.SubProviderId.HasValue)
                {
                    var subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value);
                    subProviderName = subProvider.Name.ToLower();
                }
                var balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                if (IntgrationAleaHelpers.AleaPlayHelpers.NotSupportedCurrencies.ContainsKey(subProviderName) &&
                    IntgrationAleaHelpers.AleaPlayHelpers.NotSupportedCurrencies[subProviderName].Contains(client.CurrencyId))
                {
                    if (input.currency != Constants.Currencies.USADollar)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                }
                else if (client.CurrencyId != input.currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                response = JsonConvert.SerializeObject(new
                {
                    realBalance = Math.Round(balance, 2),
                    bonusBalance = 0
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = AleaPlayHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = AleaPlayHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = AleaPlayHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    message = ex.Message
                });
                httpResponseMessage.StatusCode = AleaPlayHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }


        [HttpPost]
        [Route("{partnerId}/api/AleaPlay/transactions")]
        public HttpResponseMessage ProcessTransaction(BetInput input)
        {
            var response = string.Empty;
            var inputString = string.Empty;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Digest"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("Digest").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                WebApiApplication.DbLogger.Info("inputString: " + inputString);

                var clientSession = ClientBll.GetClientProductSession(input.casinoSessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);

                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.AleaPlaySecretKey);
                var sign = CommonFunctions.ComputeSha512($"{inputString}{secretKey}");
                if (sign.ToLower() != signature.Replace("SHA-512=", string.Empty).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.Id.ToString() != input.Player.CasinoPlayerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);               
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.ExternalId != input.Game.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var subProviderName = string.Empty;
                if (product.SubProviderId.HasValue)
                {
                    var subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value);
                    subProviderName = subProvider.Name.ToLower();
                }
                if (IntgrationAleaHelpers.AleaPlayHelpers.NotSupportedCurrencies.ContainsKey(subProviderName) &&
                    IntgrationAleaHelpers.AleaPlayHelpers.NotSupportedCurrencies[subProviderName].Contains(client.CurrencyId))
                {
                    if (input.currency != Constants.Currencies.USADollar)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    input.Amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, input.Amount);
                }
                else if (client.CurrencyId != input.currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                var document = new Document();
                switch (input.Type)
                {
                    case "BET":
                        document = DoBet(input.Id, input.Round.Id, input.Amount, client, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realAmount = decimal.Parse(string.Format("{0:N2}", input.Amount)),
                            bonusAmount = 0,
                            realBalance = decimal.Parse(string.Format("{0:N2}", Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseHelpers.GetClientProductBalance(client.Id, product.Id)), 2))),
                            bonusBalance = 0
                        });
                        break;
                    case "WIN":
                        document = DoWin(input.Id, input.Round.Id, input.Amount, client, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realAmount = decimal.Parse(string.Format("{0:N2}", input.Amount)),
                            bonusAmount = 0,
                            realBalance = decimal.Parse(string.Format("{0:N2}", Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseHelpers.GetClientProductBalance(client.Id, product.Id)), 2))),
                            bonusBalance = 0
                        });
                        break;
                    case "BET_WIN":
                        document = DoBet(input.Id, input.Id, input.Bet.Amount, client, clientSession);
                        document = DoWin(input.Id, input.Id, input.Win.Amount, client, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realBalance = decimal.Parse(string.Format("{0:N2}",  Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseHelpers.GetClientProductBalance(client.Id, product.Id)), 2))),
                            bonusBalance = 0,
                            bet = new
                            {
                                realAmount = decimal.Parse(string.Format("{0:N2}", input.Bet.Amount)),
                                bonusAmount = 0
                            },
                            win = new
                            {
                                realAmount = decimal.Parse(string.Format("{0:N2}", input.Win.Amount)),
                                bonusAmount = 0
                            }
                        });
                        break;
                    case "BET_WIN_BONUS_FREE_SPIN":
                        document = DoFreespinWin(input.Bonus.Id, input.Id, input.Win.Amount, client, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realBalance = decimal.Parse(string.Format("{0:N2}", Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseHelpers.GetClientProductBalance(client.Id, product.Id)), 2))),
                            bonusBalance = 0
                        });
                        break;
                    case "ROLLBACK":
                        document = Rollback(input.Id, input.Transaction.Id, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realBalance = decimal.Parse(string.Format("{0:N2}", Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseHelpers.GetClientProductBalance(client.Id, product.Id)), 2))),
                            bonusBalance = 0
                        });
                        break;
                }
                WebApiApplication.DbLogger.Info("response: " + response);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = AleaPlayHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = AleaPlayHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex);
                response = JsonConvert.SerializeObject(new
                {
                    code = AleaPlayHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    message = ex.Message
                });
                httpResponseMessage.StatusCode = AleaPlayHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private Document DoBet(string transactionId, string roundId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            if (amount < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.AleaRollback, transactionId);
                    if (!string.IsNullOrEmpty(rollback))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongDocumentNumber);

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId,
                        RoundId = roundId,
                        TransactionId = transactionId,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount,
                        DeviceTypeId = clientSession.DeviceType
                    });
                    betDocument =  clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);                    
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    BaseHelpers.BroadcastBetLimit(info);
                    return betDocument;
                }
            }
        }

        private Document DoWin(string transactionId, string roundId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            if (amount < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, roundId, ProviderId, client.Id);
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                    
                    var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                    var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = roundId,
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

                    winDocument =  clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];                   
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
                    return winDocument;
                }
            }
        }

        private Document DoFreespinWin(string transactionId, string roundId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            if (amount < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    transactionId = $"{Constants.FreeSpinPrefix}{transactionId}";
                    var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    var listOfOperationsFromApi = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = roundId,
                        ExternalProductId = product.ExternalId,
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        TransactionId = $"Bet_{transactionId}",
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = 0,
                        DeviceTypeId = clientSession.DeviceType
                    });
                    var betDocument = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);


                    var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = roundId,
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

                    winDocument =  clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
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
                    return winDocument;
                }
            }
        }

        private Document Rollback(string transactionId, string rollbackTransactionId, SessionIdentity clientSession)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = rollbackTransactionId,
                    ProductId = product.Id,
                    ExternalProductId = product.ExternalId
                };
                Document rollbackDocumnent;
                try
                {
                    rollbackDocumnent = documentBl.RollbackProductTransactions(operationsFromProduct, externalTransactionId: transactionId)[0];
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    if (fex.Detail.Id == Constants.Errors.DocumentNotFound)
                        CacheManager.SetFutureRollback(Constants.CacheItems.AleaRollback, transactionId, rollbackTransactionId);
                    throw;
                }
                BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                BaseHelpers.BroadcastBalance(clientSession.Id);
                return rollbackDocumnent;
            }
        }
    }
}