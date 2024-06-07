using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Models.Notification;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.WinSystems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class GeneralizedController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BAS).Id;
        private static readonly int DGSProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.DGS).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.BAS); //138.94.58.158


        [HttpPost]
        [Route("{partnerId}/api/controller/Authorization")]
        [Route("{partnerId}/api/controller/GetBalance")]
        public HttpResponseMessage Authorization(BaseInput input, [FromUri] int partnerId)
        {
            var response = new AuthorizationOutput();
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, product.GameProviderId.Value, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}|{3}", partnerId, input.ProductId, input.Token, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                if (product.ExternalId != input.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                response.ClientId = client.Id.ToString();
                response.CurrencyId = client.CurrencyId;
                response.Balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2);
                response.IsValidPlayer = true;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/controller/Credit")]
        public HttpResponseMessage DoBet(TransactionInput input, [FromUri] int partnerId)
        {
            var response = new TransactionOutput();
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, product.GameProviderId.Value, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", partnerId, input.ProductId, input.TransactionId,
                                                                                                    input.Amount, input.Token, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);              
                var client = CacheManager.GetClientById(clientSession.Id);
              
                if (product.ExternalId != input.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (client.CurrencyId != input.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var amount = Convert.ToDecimal(input.Amount);
                if (amount < 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, product.GameProviderId.Value,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (betDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = product.GameProviderId.Value,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId,
                            RoundId = input.RoundId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                        response.ClientId = client.Id.ToString();
                        response.CurrencyId = client.CurrencyId;
                        response.Balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2);
                        response.BetId = betDocument.Id.ToString();
                        response.IsValidPlayer = true;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/controller/Debit")]
        public HttpResponseMessage DoWin(TransactionInput input, [FromUri] int partnerId)
        {
            var response = new TransactionOutput();
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);              
                BllClient client = null;
                BllProduct product = null;
                if (!string.IsNullOrEmpty(input.Token))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                    client = CacheManager.GetClientById(clientSession.Id);
                    product = CacheManager.GetProductById(clientSession.ProductId);
                    if (product.ExternalId != input.ProductId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                }
                else
                {
                    client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId)) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                    if (product == null)
                        product = CacheManager.GetProductByExternalId(DGSProviderId, input.ProductId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                }
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, product.GameProviderId.Value, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", partnerId, input.ProductId, input.TransactionId,
                                                                                                        input.CreditTransactionId ?? input.BetId, input.Amount,
                                                                                                        input.Token ?? input.ClientId, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                var amount = Convert.ToDecimal(input.Amount);
                if (amount < 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        Document betDocument = null;
                        if (!string.IsNullOrEmpty(input.CreditTransactionId))
                         betDocument = documentBl.GetDocumentByExternalId(input.CreditTransactionId, product.GameProviderId.Value, product.Id, OperationTypes.Bet, null);
                        else if(Int64.TryParse(input.BetId, out long betId))
                            betDocument = documentBl.GetDocumentById(betId);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
           
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, product.GameProviderId.Value,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        betDocument.State = state;

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            GameProviderId = product.GameProviderId.Value,
                            OperationTypeId = (int)OperationTypes.Win,
                            TransactionId = input.TransactionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount
                        });
                        var winDocuments = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

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
                        response.TransactionId = winDocuments[0].Id.ToString();
                        response.ClientId = client.Id.ToString();
                        response.CurrencyId = client.CurrencyId;
                        response.Balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2);
                        response.BetId = betDocument.Id.ToString();
                        response.IsValidPlayer = true;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/controller/BonusWin")]
        public HttpResponseMessage DoBonusWin(TransactionInput input, [FromUri] int partnerId)
        {
            var response = new TransactionOutput();
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}|{3}|{4}", partnerId, input.TransactionId, input.Amount,
                                                                                                        input.ClientId, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

               var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId)) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                if (client.CurrencyId != input.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                var amount = Convert.ToDecimal(input.Amount);
                if (amount < 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, product.GameProviderId.Value,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                        var betOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = product.GameProviderId.Value,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId + "_BonusBet",
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Won,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl, out LimitInfo info);                     

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            GameProviderId = product.GameProviderId.Value,
                            OperationTypeId = (int)OperationTypes.Win,
                            TransactionId = input.TransactionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount
                        });
                        var winDocuments = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

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
                        response.TransactionId = winDocuments[0].Id.ToString();
                        response.ClientId = client.Id.ToString();
                        response.CurrencyId = client.CurrencyId;
                        response.Balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2);
                        response.BetId = betDocument.Id.ToString();
                        response.IsValidPlayer = true;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }


        [HttpPost]
        [Route("{partnerId}/api/controller/Rollback")]
        public HttpResponseMessage Rollback(TransactionInput input, [FromUri] int partnerId)
        {
            WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
            var response = new TransactionOutput();
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.RollbackTransactionId ?? input.BetId, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                // ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                if (product == null)
                    product = CacheManager.GetProductByExternalId(DGSProviderId, input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    List<Document> rollbackedDocument = null;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = product.GameProviderId.Value,
                        ProductId = product.Id,
                        TransactionId = input.RollbackTransactionId,
                        OperationTypeId = input.OperationTypeId
                    };
                    if (string.IsNullOrEmpty(input.RollbackTransactionId))
                    {
                        if (!Int64.TryParse(input.BetId, out long betId) ||
                          (input.OperationTypeId != (int)OperationTypes.Bet && input.OperationTypeId != (int)OperationTypes.Win))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongDocumentNumber);
                        if (input.OperationTypeId == (int)OperationTypes.Bet)
                        {
                            var betDocument = documentBl.GetDocumentById(betId)??
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                            operationsFromProduct.TransactionId = betDocument.ExternalTransactionId;
                            rollbackedDocument = documentBl.RollbackProductTransactions(operationsFromProduct);
                        }
                        else
                        {
                            var winDocuments = documentBl.GetBetRelatedDocuments(betId)??
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                            foreach (var winDoc in winDocuments)
                            {
                                operationsFromProduct.TransactionId = winDoc.ExternalTransactionId;
                                rollbackedDocument = documentBl.RollbackProductTransactions(operationsFromProduct);
                            }
                        }


                    }
                    else
                        rollbackedDocument = documentBl.RollbackProductTransactions(operationsFromProduct);

                    BaseHelpers.RemoveClientBalanceFromeCache(rollbackedDocument[0].ClientId.Value);
                    var client = CacheManager.GetClientById(rollbackedDocument[0].ClientId.Value);
                    response.ClientId = client.Id.ToString();
                    response.CurrencyId = client.CurrencyId;
                    response.Balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2);
                    response.TransactionId = rollbackedDocument[0].Id.ToString();
                    response.IsValidPlayer = true;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/controller/GetPlayerInfo")]
        public HttpResponseMessage GetPlayerInfo(PlayerInfoInput input, [FromUri] int partnerId)
        {
            var response = new ApiResponseBase();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.PlayerId, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (!int.TryParse(input.PlayerId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var client = CacheManager.GetClientById(clientId);
                if (client == null || client.PartnerId != partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientIdentities = clientBl.GetClientIdentities(client.Id)
                                                  .OrderByDescending(x => x.Status == (int)KYCDocumentStates.Approved)
                                                  .ThenByDescending(x => x.Status ==(int)KYCDocumentStates.InProcess)
                                                  .ThenByDescending(x => x.Status);
                    var utilityBill = clientIdentities.FirstOrDefault(x => x.DocumentTypeId == (int)KYCDocumentTypes.UtilityBill);
                    var idCard = clientIdentities.FirstOrDefault(x => x.DocumentTypeId == (int)KYCDocumentTypes.IDCard);
                    response.ResponseObject = new
                    {
                        PlayerId = client.Id.ToString(),
                        PlayerCurrency = client.CurrencyId,
                        AvailableBalance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, 0), 2),
                        client.MobileNumber,
                        client.Email,
                        AddressStatus = utilityBill== null ? 0 : utilityBill.Status,
                        OMANGStatus = idCard== null ? 0 : idCard.Status,
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/controller/GetClient")]
        public HttpResponseMessage GetClient(ClientInput input, [FromUri] int partnerId)
        {
            var response = new ApiResponseBase();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BASApiKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.MobileNumber, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var client = CacheManager.GetClientByMobileNumber(partnerId, input.MobileNumber);
                if (client == null)
                {
                    if (input.MobileNumber.Contains("+"))
                        input.MobileNumber = input.MobileNumber.Replace("+", string.Empty);
                    else
                        input.MobileNumber = "+" + input.MobileNumber;
                    client = CacheManager.GetClientByMobileNumber(partnerId, input.MobileNumber);
                    if (client != null)
                    {
                        response.ResponseCode = 0;
                        response.ResponseObject = true;
                    }
                    else
                    {
                        response.ResponseCode = Constants.Errors.ClientNotFound;
                        response.ResponseObject = false;
                    }
                }
                else
                {
                    response.ResponseCode = 0;
                    response.ResponseObject = true;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }       

        [Route("{partnerId}/api/controller/SMS")]
        public HttpResponseMessage SendSMS(NotificationInput input, [FromUri] int partnerId)
        {
            var responseBase = new ApiResponseBase { ResponseCode = 0, Description = "Success" };
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                SendNotification(partnerId, ClientMessageTypes.Sms, input);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                responseBase.ResponseCode = fex.Detail.Id;
                responseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                responseBase.ResponseCode = Constants.Errors.GeneralException; ;
                responseBase.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseBase), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [Route("{partnerId}/api/controller/Email")]
        public HttpResponseMessage SendEmail(NotificationInput input, [FromUri] int partnerId)
        {
            var responseBase = new ApiResponseBase { ResponseCode = 0, Description = "Success" };
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                SendNotification(partnerId, ClientMessageTypes.Email, input);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                responseBase.ResponseCode = fex.Detail.Id;
                responseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                responseBase.ResponseCode = Constants.Errors.GeneralException; ;
                responseBase.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseBase), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [Route("{partnerId}/api/controller/Notification")]
        public HttpResponseMessage SendTicket(NotificationInput input, [FromUri] int partnerId)
        {
            var responseBase = new ApiResponseBase { ResponseCode = 0, Description = "Success" };
            try
            {
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                SendNotification(partnerId, ClientMessageTypes.MessageFromSystem, input);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                responseBase.ResponseCode = fex.Detail.Id;
                responseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                responseBase.ResponseCode = Constants.Errors.GeneralException; ;
                responseBase.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseBase), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private void SendNotification(int partnerId, ClientMessageTypes type, NotificationInput input)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BASApiKey);
            var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.ClientId, partnerKey));
            if (signature.ToLower() != input.Signature)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            if (!int.TryParse(input.ClientId, out int clientId))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var client = CacheManager.GetClientById(clientId);
            if (client == null || client.PartnerId != partnerId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            if (input.Type.HasValue && !Enum.IsDefined(typeof(ClientInfoTypes), input.Type))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
            using (var notificationBll = new NotificationBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                if (type == ClientMessageTypes.Email || type == ClientMessageTypes.Sms)
                    notificationBll.SendNotificationMessage(new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ObjectId = client.Id,
                        ObjectTypeId = (int)ObjectTypes.Client,
                        MobileOrEmail = type == ClientMessageTypes.Email ? client.Email : client.MobileNumber,
                        ClientInfoType = input.Type,
                        MessageText = input.Message,
                        MessageType = (int)type
                    }, out int responseCode);
                else
                    notificationBll.SendInternalTicket(client.Id, input.Type, input.Message);
            }
        }
    }
}