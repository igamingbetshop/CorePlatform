using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Kiron;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class KironController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Kiron).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Kiron);

        [HttpPost]
        [Route("{partnerId}/api/kiron/activateSession")]
        public HttpResponseMessage CheckSession(BaseInput input)
        {
            var response = new AuthenticationOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.PlayerToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);

                response.PlayerID = client.Id.ToString();
                response.CurrencyCode = client.CurrencyId;
                response.LanguageCode = clientSession.LanguageId;

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/kiron/getBalance")]
        public HttpResponseMessage GetBalance(GetBalanceInput input)
        {
            var response = new BalanceOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.PlayerToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.Id.ToString() != input.PlayerID)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    response.Amount = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    response.Amount = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/kiron/debit")]
        public HttpResponseMessage DoBet(DebitInput input)
        {
            var response = new TransactionOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.PlayerToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.Id.ToString() != input.PlayerID)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                if (client.CurrencyId != input.CurrencyCode)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        BllProduct product;
                        if (input.GameIds != null && input.GameIds.Count !=0)
                        {
                            product = CacheManager.GetProductByExternalId(ProviderId, input.GameIds[0].ToString());
                            if (product == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        }
                        else 
                            product = CacheManager.GetProductById(clientSession.ProductId);

                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(input.BetManTransactionID, client.Id,
                              ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (betDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = input.BetManTransactionID,
                            RoundId = input.RoundID,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                var balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 
                                    clientSession.ParentId ?? 0, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                                BaseHelpers.BroadcastBalance(client.Id, balance);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        else
                        {
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }
                        response.TransactionID = betDocument.Id.ToString();
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/kiron/credit")]
        public HttpResponseMessage DoWin(CreditInput input)
        {
            var response = new TransactionOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.PlayerToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.Id.ToString() != input.PlayerID)
                  throw  BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                if (client.CurrencyId != input.CurrencyCode)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {                    
                        var betDocument = documentBl.GetDocumentById(Convert.ToInt64(input.PreviousTransactionID));
                        if (betDocument == null || betDocument.OperationTypeId != (int)OperationTypes.Bet)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, betDocument.ProductId.Value);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var winDocument = documentBl.GetDocumentByExternalId(input.BetManTransactionID, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var product = CacheManager.GetProductById(betDocument.ProductId.Value);
                            var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;

                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundID,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                TransactionId =  input.BetManTransactionID,
                                ExternalProductId = product.ExternalId,
                                ProductId = betDocument.ProductId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount
                            });
                            var winDocuments = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                        betDocument.Id, operationsFromProduct, winDocuments[0], WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                                }
                            }
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new Common.Models.WebSiteModels.ApiWin
                                {
                                    BetId = betDocument?.Id ?? 0,
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    BetAmount = betDocument?.Amount,
                                    Amount = input.Amount,
                                    CurrencyId = client.CurrencyId,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                            response.TransactionID = winDocuments[0].Id.ToString();
                        }
                        else
                            response.TransactionID = winDocument.Id.ToString();
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                WebApiApplication.DbLogger.Error(fex);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                WebApiApplication.DbLogger.Error(ex);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/kiron/finalizeRound")]
        public HttpResponseMessage FinalizeRound(FinalizeRoundInput input)
        {
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.PlayerToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.Id.ToString() != input.PlayerID)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundID, ProviderId,
                                                                        client.Id, (int)BetDocumentStates.Uncalculated);
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            State = (int)BetDocumentStates.Lost,
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundID,
                            GameProviderId = ProviderId,                            
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
                            listOfOperationsFromApi.ProductId = betDoc.ProductId;
                            clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl);
                        }                       
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }
        [HttpPost]
        [Route("{partnerId}/api/kiron/rollback")]
        public HttpResponseMessage Rollback(DebitInput input)
        {
            var response = new TransactionOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.PlayerToken, Constants.DefaultLanguageId, null, false);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.Id.ToString() != input.PlayerID)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Bet,
                        TransactionId = input.BetManTransactionID
                    };
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct, false);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), 
                                client, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex.Message);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    response.TransactionID = doc[0].Id.ToString();
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }


    }
}