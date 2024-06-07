using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.TurboGames;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.IO;
using System.Web;
using IqSoft.CP.DAL;
using System.Net.Http.Headers;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class TurboGamesController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TurboGames).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.TurboGames);
        private enum TransactionTypes
        {
            Bet = 1,
            Win = 2,
            Rollback = 3
        }

        [HttpPost]
        [Route("{partnerId}/api/TurboGames/user/profile")]
        public HttpResponseMessage Authenticate(BaseInput input)
        {
            var response = string.Empty;
            var inputString = string.Empty;

            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                CheckSignature(client.PartnerId, inputString);
                response = JsonConvert.SerializeObject(new
                {
                    userId = client.Id.ToString(),
                    currency = client.CurrencyId,
                    currencies = new List<string> { client.CurrencyId },
                    isTest = true,
                    customFields = new
                    {
                        username = client.UserName
                    }
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;

        }

        [HttpPost]
        [Route("{partnerId}/api/TurboGames/user/balance")]
        public HttpResponseMessage GetBalance(BalanceInput input)
        {
            var response = string.Empty;
            var inputString = string.Empty;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {                
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                CheckSignature(client.PartnerId, inputString);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response = JsonConvert.SerializeObject(new
                {
                    userId = client.Id.ToString(),
                    currency = client.CurrencyId,
                    amount = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2)
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/TurboGames/payment/check")]
        public HttpResponseMessage CheckTransaction(TransactionInput input)
        {
            var response = string.Empty;
            var inputString = string.Empty;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                // BaseBll.CheckIp(WhitelistedIps);
                if (!int.TryParse(input.ClientId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                CheckSignature(client.PartnerId, inputString);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var betDocument = documentBl.GetDocumentOnlyByExternalId(input.TransactionId, ProviderId, client.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongDocumentId);
                    response = JsonConvert.SerializeObject(new
                    {
                        transactionId = betDocument.Id.ToString(),
                        currency = client.CurrencyId,
                        amount = betDocument.Amount,
                        type = (int)TransactionTypes.Bet,
                        transactionTime = betDocument.CreationTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
                    });
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost, HttpPut]
        [Route("{partnerId}/api/TurboGames/payment/bet")]
        public HttpResponseMessage ProcessTransaction(TransactionInput input)
        {
            var response = string.Empty;
            var inputString = string.Empty;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: input.Type == 1);
                var client = CacheManager.GetClientById(clientSession.Id);
                CheckSignature(client.PartnerId, inputString);

                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if(!input.Amount.HasValue)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                Document transactionDocument;
                switch (input.Type)
                {
                    case (int)TransactionTypes.Bet:
                        transactionDocument = DoBet(input.TransactionId, input.Amount.Value, client, clientSession);
                        break;
                    case (int)TransactionTypes.Win:
                        transactionDocument = DoWin(input.RequestId, input.TransactionId, input.Amount.Value, client, clientSession);
                        break;
                    case (int)TransactionTypes.Rollback:
                        transactionDocument = Rollback(input.TransactionId, clientSession);
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                }
                response = JsonConvert.SerializeObject(new
                {
                    transactionId = string.Format("transaction_{0}", transactionDocument?.Id.ToString()),
                    transactionTime = transactionDocument != null ? transactionDocument.CreationTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK") : DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private Document DoBet(string transactionId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
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

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId,
                        RoundId = transactionId,
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

        private Document DoWin(string transactionId, string betTransactionId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(betTransactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Bet);
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

                    winDocument =  clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
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
                    return winDocument;
                }
            }
        }

        private Document Rollback(string transactionId, SessionIdentity clientSession)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                    return rollbackDocumnent;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
                    throw;
                return null;
            }
        }

        private static string GenerateJWTToken(int partnerId, string request)
        {
            var key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TurboGamesApiKey);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var payload = JwtPayload.Deserialize(request);            
            var headers = new JwtHeader(signingCredentials);
            if (headers.ContainsKey("cty"))
                headers.Remove("cty");
            var secToken = new JwtSecurityToken(headers, payload);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }

        private void CheckSignature(int partnerId, string request)
        {
            if (!Request.Headers.Contains("x-sign-jws"))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            var jws = Request.Headers.GetValues("x-sign-jws").FirstOrDefault();
            if (string.IsNullOrEmpty(jws))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            WebApiApplication.DbLogger.Info("x-sign-jws: " + jws);
           var token = GenerateJWTToken(partnerId, request);
            if (jws.ToString().Split('.')[2] != token.Split('.')[2])
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }
    }
}