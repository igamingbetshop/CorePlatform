using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.DragonGaming;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Net;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using System.Linq;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class DragonGamingController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.DragonGaming).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.DragonGaming);

        [HttpPost]
        [Route("{partnerId}/api/DragonGaming/get_session/")]
        public HttpResponseMessage GetSession(BaseInput input)
        {
            var response = new BaseOutput { Status = 1 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                response.ClientId = client.Id.ToString();
                response.Username = client.UserName;
                response.Country = clientSession.Country;
                response.Token = input.Token.ToString();
                response.Balance = (int)(BaseHelpers.GetClientProductBalance(clientSession.Id, clientSession.ProductId) * 100);
                response.Currency = client.CurrencyId;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                var error = DragonGamingHelpers.GetErrorCode(fex.Detail.Id);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                var error = DragonGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/DragonGaming/get_balance/")]
        public HttpResponseMessage GetBalance(BaseInput input)
        {
            var response = new BaseOutput { Status = 1 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                if (clientSession.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                response.ClientId = client.Id.ToString();
                response.Country = clientSession.Country;
                response.Token = input.ToString();
                response.Balance = (int)(BaseHelpers.GetClientProductBalance(clientSession.Id, clientSession.ProductId) * 100);
                response.Currency = client.CurrencyId;
                response.BonusAmount = 0;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                var error = DragonGamingHelpers.GetErrorCode(fex.Detail.Id);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                var error = DragonGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/DragonGaming/debit/")]
        public HttpResponseMessage DoBet(BetInput input)
        {
            var response = new BaseOutput { Status = 1 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                if (clientSession.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (!product.ExternalId.Contains(input.GameId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var document = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.TransactionId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount / 100m,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            var doc = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            response.TransactionId = doc.Id.ToString();
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        else
                            response.TransactionId = document.Id.ToString();
                        response.ClientId = client.Id.ToString();
                        response.Country = clientSession.Country;
                        response.Token = input.Token.ToString();
                        response.Balance = (int)(BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id) * 100);
                        response.Currency = client.CurrencyId;
                        response.BonusAmount = 0;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                var error = DragonGamingHelpers.GetErrorCode(fex.Detail.Id);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                var error = DragonGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/DragonGaming/credit/")]
        public HttpResponseMessage DoWin(BetInput input)
        {
            WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
            var response = new BaseOutput { Status = 1 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                if (clientSession.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (!product.ExternalId.Contains(input.GameId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var state = input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ProductId = product.Id,
                                TransactionId = input.TransactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount / 100m,
                                DeviceTypeId = clientSession.DeviceType
                            });

                            var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = input.Amount / 100m,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                            response.TransactionId = doc[0].Id.ToString();
                        }
                        else
                            response.TransactionId = winDocument.Id.ToString();
                        response.ClientId = client.Id.ToString();
                        response.Country = clientSession.Country;
                        response.Token = input.Token.ToString();
                        response.Balance = (int)(BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id) * 100);
                        response.Currency = client.CurrencyId;
                        response.BonusAmount = 0;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                var error = DragonGamingHelpers.GetErrorCode(fex.Detail.Id);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                var error = DragonGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }


        [HttpPost]
        [Route("{partnerId}/api/Mancala/refund/")]
        public HttpResponseMessage Rollback(BetInput input)
        {
            WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
            var response = new BaseOutput { Status = 1 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                if (clientSession.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.ExternalId != input.GameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            GameProviderId = ProviderId,
                            TransactionId = input.OriginalTransactionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id
                        };
                        var documents = new List<Document>();
                        try
                        {
                            documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        }
                        catch { }
                        BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                        response.ClientId = client.Id.ToString();
                        response.Country = clientSession.Country;
                        response.Token = input.ToString();
                        response.Balance = (int)(BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id) * 100);
                        response.Currency = client.CurrencyId;
                        response.TransactionId = (documents != null && documents.Any()) ? documents[0].Id.ToString() : string.Empty;
                        response.BonusAmount = 0;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                var error = DragonGamingHelpers.GetErrorCode(fex.Detail.Id);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                var error = DragonGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = 0;
                response.ErrorId = (int)error;
                response.ErrorCode = error.ToString();
                response.ErrorMessage =  ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}