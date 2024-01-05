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
using Microsoft.AspNetCore.Mvc;
using IntgrationAleaHelpers = IqSoft.CP.Integration.Products.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class AleaPlayController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.AleaPlay).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "63.35.38.133",
            "52.214.240.56",
            "3.248.123.49",
            "99.80.177.127"
        };

        [HttpGet]
        [Route("{partnerId}/api/AleaPlay/players/{casinoPlayerId}/balance")]
        public ActionResult GetBalance([FromQuery]BaseInput input, [FromRoute]string casinoPlayerId)
        {
            object response;
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.ContainsKey("Digest"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers["Digest"];
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
                if (sign.ToLower() != signature.ToString().Replace("SHA-512=", string.Empty).ToLower())
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
                var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;

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
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response = new
                {
                    code = AleaPlayHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                };
                statusCode = AleaPlayHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response = new
                {
                    code = AleaPlayHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    message = ex.Message
                };
                statusCode = AleaPlayHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            if (statusCode == HttpStatusCode.Unauthorized)
                return Unauthorized(response);
            if (statusCode == HttpStatusCode.Forbidden)
                return Forbid(JsonConvert.SerializeObject(response));
            return BadRequest(response);
        }


        [HttpPost]
        [Route("{partnerId}/api/AleaPlay/transactions")]
        public ActionResult ProcessTransaction(BetInput input)
        {
            var response = new object();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            var inputString = string.Empty;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.ContainsKey("Digest"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers["Digest"];
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                var clientSession = ClientBll.GetClientProductSession(input.casinoSessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);

                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.AleaPlaySecretKey);
                var sign = CommonFunctions.ComputeSha512($"{inputString}{secretKey}");
                if (sign.ToLower() != signature.ToString().Replace("SHA-512=", string.Empty).ToLower())
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
                            realAmount = input.Amount,
                            bonusAmount = 0,
                            realBalance = Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance), 2),
                            bonusBalance = 0
                        });
                        break;
                    case "WIN":
                        document = DoWin(input.Id, input.Round.Id, input.Amount, client, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realAmount = input.Amount,
                            bonusAmount = 0,
                            realBalance = Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance), 2),
                            bonusBalance = 0
                        });
                        break;
                    case "BET_WIN":
                        document = DoBet(input.Id, input.Id, input.Bet.Amount, client, clientSession);
                        document = DoWin(input.Id, input.Id, input.Win.Amount, client, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realBalance = Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance), 2),
                            bonusBalance = 0,
                            bet = new
                            {
                                realAmount = input.Bet.Amount,
                                bonusAmount = 0
                            },
                            win = new
                            {
                                realAmount = input.Win.Amount,
                                bonusAmount = 0
                            }
                        });
                        break;
                    case "ROLLBACK":
                        document = Rollback(input.Transaction.Id, clientSession);
                        response = JsonConvert.SerializeObject(new
                        {
                            id = document.Id.ToString(),
                            realBalance = Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, input.currency, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance), 2),
                            bonusBalance = 0
                        });
                        break;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response = new
                {
                    code = AleaPlayHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                };
                statusCode = AleaPlayHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response = new
                {
                    code = AleaPlayHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    message = ex.Message
                };
                statusCode = AleaPlayHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            if (statusCode == HttpStatusCode.Unauthorized)
                return Unauthorized(response);
            if (statusCode == HttpStatusCode.Forbidden)
                return Forbid(JsonConvert.SerializeObject(response));
            return BadRequest(response);
        }

        private static Document DoBet(string transactionId, string roundId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            if (amount <= 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                    betDocument =  clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    return betDocument;
                }
            }
        }

        private static Document DoWin(string transactionId, string roundId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            if (amount < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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

        private static Document Rollback(string transactionId, SessionIdentity clientSession)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = transactionId,
                    ProductId = product.Id,
                    ExternalProductId = product.ExternalId
                };
                var rollbackDocumnent = documentBl.RollbackProductTransactions(operationsFromProduct)[0];
                BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                BaseHelpers.BroadcastBalance(clientSession.Id);
                return rollbackDocumnent;
            }
        }
    }
}