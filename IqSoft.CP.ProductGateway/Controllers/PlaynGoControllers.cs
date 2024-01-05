using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.PlaynGo;
using Input = IqSoft.CP.ProductGateway.Models.PlaynGo.Input;
using Output = IqSoft.CP.ProductGateway.Models.PlaynGo.Output;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{

    public class PlaynGoControllers : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.PlaynGo);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.PlaynGo).Id;


        [HttpPost]
        [Route("{partnerId}/api/playngo/authenticate")]
        public HttpResponseMessage Authenticate(AuthenticationInput input)
        {
            var authenticationOutput = new Authenticate();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.username, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);

                authenticationOutput.externalId = client.Id.ToString();
                authenticationOutput.userCurrency = client.CurrencyId;
                authenticationOutput.nickname = client.UserName;
                authenticationOutput.country = clientSession.Country;
                authenticationOutput.birthdate= client.BirthDate.ToString("yyyy-MM-dd");
                authenticationOutput.registration = client.BirthDate.ToString("yyyy-MM-dd");
                authenticationOutput.language = clientSession.LanguageId;
                authenticationOutput.real =  Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2);
                authenticationOutput.externalGameSessionId = clientSession.Token;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                authenticationOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                authenticationOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail);
            }
            catch (Exception ex)
            {
                authenticationOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                authenticationOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CommonFunctions.ToXML(authenticationOutput), Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/playngo/balance")]
        public HttpResponseMessage GetBalance(Input.Balance input)
        {
            var balanceOutput = new Output.Balance();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);

                balanceOutput.real =  Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2).ToString("F");
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                balanceOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                balanceOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail);
            }
            catch (Exception ex)
            {
                balanceOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                balanceOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CommonFunctions.ToXML(balanceOutput), Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/playngo/reserve")]
        public HttpResponseMessage Reserve(Input.Reserve input)
        {
            var reserveOutput = new Output.Reserve();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId || product.ExternalId != input.gameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var document = documentBl.GetDocumentByExternalId(input.transactionId, client.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.roundId,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.transactionId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.real,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            reserveOutput.externalTransactionId= clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info).Id.ToString();
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                    }
                }
                reserveOutput.real = BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id).ToString("F");

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                reserveOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                reserveOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail);
            }
            catch (Exception ex)
            {
                reserveOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                reserveOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CommonFunctions.ToXML(reserveOutput), Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/playngo/release")]
        public HttpResponseMessage Release(Input.Release input)
        {
            var releaseOutput = new Output.Release();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId || product.ExternalId != input.gameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.roundId, ProviderId, client.Id, (int)DocumentStates.Uncalculated);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.transactionId, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var state = input.real > 0 ? (int)DocumentStates.Won : (int)DocumentStates.Lost;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.roundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ProductId = product.Id,
                                TransactionId = input.transactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.real,
                                DeviceTypeId = clientSession.DeviceType
                            });

                            releaseOutput.externalTransactionId = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0].Id.ToString();
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = input.real,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                    }
                }
                releaseOutput.real = BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id).ToString("F");
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                releaseOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                releaseOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail);
            }
            catch (Exception ex)
            {
                releaseOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                releaseOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CommonFunctions.ToXML(releaseOutput), Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/playngo/cancelreserve")]
        public HttpResponseMessage CancelReserve(Input.CancelReserve input)
        {
            var cancelReleaseOutput = new Output.CancelRelease();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId || product.ExternalId != input.gameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            GameProviderId = ProviderId,
                            TransactionId = input.transactionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id
                        };
                        try
                        {
                            cancelReleaseOutput.externalTransactionId = documentBl.RollbackProductTransactions(operationsFromProduct)[0].Id.ToString();
                        }
                        catch (FaultException<BllFnErrorType>)
                        {
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                    }
                }
                cancelReleaseOutput.real = BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id).ToString("F");
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                cancelReleaseOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                cancelReleaseOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail);
            }
            catch (Exception ex)
            {
                cancelReleaseOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                cancelReleaseOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CommonFunctions.ToXML(cancelReleaseOutput), Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }
    }
}