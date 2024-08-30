using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.ProductGateway.Models.BetMakers;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net.Http.Headers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;

namespace IqSoft.CP.ProductGateway
{
    public class BetMakersController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetMakers).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.BetMakers);
        private static readonly string ProductExternalId = "BetMakers";
        [HttpPost]
        [Route("{partnerId}/api/BetMakers/reserve")]
        public HttpResponseMessage ReserveBet(BaseInput reserveInput)
        {
            string response;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Authorization")) // check
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var headerApiKey = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(headerApiKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var clientSession = ClientBll.GetClientProductSession(reserveInput.Token, Constants.DefaultLanguageId);
                var userUuid = new Guid(reserveInput.UserId);
                if (clientSession.Id.ToString() != reserveInput.Metadata.BrandUserId || clientSession.Id != CommonFunctions.DecodeNumberFromGuid(userUuid))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetMakersApiKey);
                if (apiKey != headerApiKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                DoBet(reserveInput.BetId, reserveInput.Amount, client, clientSession);

                response = JsonConvert.SerializeObject(new { success = true });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(reserveInput) + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = fex.Detail.Message,
                    code = fex.Detail.Id,
                    statusCode = 400 // ??
                });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(reserveInput) + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = ex.Message,
                    code = Constants.Errors.GeneralException,
                    statusCode = 400
                });
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/BetMakers/confirm")]
        public HttpResponseMessage ConfirmBet(BaseInput confirmInput)
        {
            string response;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Authorization"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var headerApiKey = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(headerApiKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var userUuid = new Guid(confirmInput.UserId);
                var clientId = CommonFunctions.DecodeNumberFromGuid(userUuid);
                var client = CacheManager.GetClientById((int)clientId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetMakersApiKey);
                if (apiKey != headerApiKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, ProductExternalId)??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var betDocument = documentBl.GetDocumentByExternalId(confirmInput.BetId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Bet) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                }
                response = JsonConvert.SerializeObject(new { success = true });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(confirmInput) + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = fex.Detail.Message,
                    code = fex.Detail.Id,
                    statusCode = 400
                });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(confirmInput) + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = ex.Message,
                    code = Constants.Errors.GeneralException,
                    statusCode = 400
                });
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/BetMakers/refundBet")]
        [Route("{partnerId}/api/BetMakers/debit")]
        public HttpResponseMessage Rollback(BaseInput refundInput)
        {
            string response;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Authorization"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var headerApiKey = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(headerApiKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var userUuid = new Guid(refundInput.UserId);
                var clientId = CommonFunctions.DecodeNumberFromGuid(userUuid);
                var client = CacheManager.GetClientById((int)clientId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetMakersApiKey);
                if (apiKey != headerApiKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, ProductExternalId)??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                RollbackTransaction(refundInput.BetId, client.Id, string.IsNullOrEmpty(refundInput.TransactionType) ?  OperationTypes.Bet : OperationTypes.Win);
                response = JsonConvert.SerializeObject(new { success = true });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(refundInput) + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = fex.Detail.Message,
                    code = fex.Detail.Id,
                    statusCode = 400
                });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(refundInput) + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = ex.Message,
                    code = Constants.Errors.GeneralException,
                    statusCode = 400
                });
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/BetMakers/settleBet")]
        public HttpResponseMessage SettleBet(BaseInput settleBetInput)
        {
            string response;
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Authorization"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var headerApiKey = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(headerApiKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var userUuid = new Guid(settleBetInput.UserId);
                var clientId = CommonFunctions.DecodeNumberFromGuid(userUuid);
                var client = CacheManager.GetClientById((int)clientId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetMakersApiKey);
                if (apiKey != headerApiKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                DoWin(settleBetInput.BetId, settleBetInput.Amount, client);
                response = JsonConvert.SerializeObject(new { success = true });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(settleBetInput) + "_   ErrorMessage: " + fex.Detail.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = fex.Detail.Message,
                    code = fex.Detail.Id,
                    statusCode = 400
                });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(settleBetInput) + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new
                {
                    message = ex.Message,
                    code = Constants.Errors.GeneralException,
                    statusCode = 400
                });
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private void DoBet(string transactionId, decimal amount, BllClient client, SessionIdentity clientSession)
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
                    clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    BaseHelpers.BroadcastBetLimit(info);
                }
            }
        }

        private void DoWin(string transactionId, decimal amount, BllClient client)
        {
            if (amount < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            var product = CacheManager.GetProductByExternalId(ProviderId, ProductExternalId)??
                  throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            using (var clientBl = new ClientBll(documentBl))
            {
                var betDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                     partnerProductSetting.Id, (int)OperationTypes.Bet) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                if (betDocument.State != (int)BetDocumentStates.Uncalculated)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
                var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                betDocument.State = state;
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = betDocument.SessionId,
                    CurrencyId = client.CurrencyId,
                    RoundId = betDocument.RoundId,
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

        private void RollbackTransaction(string transactionId, int clientId, OperationTypes operationType)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var product = CacheManager.GetProductByExternalId(ProviderId, ProductExternalId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = transactionId,
                    ExternalProductId = ProductExternalId,
                    ProductId = product.Id,
                    OperationTypeId = (int)operationType
                };
                documentBl.RollbackProductTransactions(operationsFromProduct);
                BaseHelpers.RemoveClientBalanceFromeCache(clientId);
            }
        }
    }
}