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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.IO;
using System.Xml.Serialization;
using IqSoft.CP.Integration;

namespace IqSoft.CP.ProductGateway.Controllers
{

    public class PlaynGoController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.PlaynGo);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.PlaynGo).Id;


        [HttpPost]
        [Route("{partnerId}/api/playngo/authenticate")]
        public HttpResponseMessage Authenticate(HttpRequestMessage httpRequestMessage)
        {
            var authenticationOutput = new Output.Authenticate();
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("Input: " + inputString);
                var serializer = new XmlSerializer(typeof(AuthenticateInput), new XmlRootAttribute("authenticate"));
                var input = (AuthenticateInput)serializer.Deserialize(new StringReader(inputString));
               // BaseBll.CheckIp(WhitelistedIps);
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
                authenticationOutput.birthdate = client.BirthDate.ToString("yyyy-MM-dd");
                authenticationOutput.registration = client.BirthDate.ToString("yyyy-MM-dd");
                authenticationOutput.language = CommonHelpers.LanguageISOCodes[ clientSession.LanguageId];
                authenticationOutput.real =  Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2);
                authenticationOutput.externalGameSessionId = clientSession.Token;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                authenticationOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                authenticationOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
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
        public HttpResponseMessage GetBalance(HttpRequestMessage httpRequestMessage)
        {
            var balanceOutput = new Output.Balance();
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("Input: " + inputString);
                var serializer = new XmlSerializer(typeof(Input.Balance), new XmlRootAttribute("balance"));
                var input = (Input.Balance)serializer.Deserialize(new StringReader(inputString));
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);

                balanceOutput.real =  Math.Round(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId), 2);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                balanceOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                balanceOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
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
        public HttpResponseMessage Reserve(HttpRequestMessage httpRequestMessage)
        {
            var reserveOutput = new Output.Reserve();
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("Input: " + inputString);
                var serializer = new XmlSerializer(typeof(Input.Reserve), new XmlRootAttribute("reserve"));
                var input = (Input.Reserve)serializer.Deserialize(new StringReader(inputString));
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId )
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                if (string.IsNullOrEmpty(input.freegameExternalId))
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
                                reserveOutput.externalTransactionId = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info).Id.ToString();
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                                BaseHelpers.BroadcastBetLimit(info);
                            }
                        }
                    }
                reserveOutput.real = Math.Round(BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id), 2);

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                reserveOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                reserveOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                reserveOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                reserveOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var output = CommonFunctions.ToXML(reserveOutput);
            WebApiApplication.DbLogger.Info("Output: " + output);
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(output, Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/playngo/release")]
        public HttpResponseMessage Release(HttpRequestMessage httpRequestMessage)
        {
            var releaseOutput = new Output.Release();
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("Input: " + inputString);
                var serializer = new XmlSerializer(typeof(Input.Release), new XmlRootAttribute("release"));
                var input = (Input.Release)serializer.Deserialize(new StringReader(inputString));

                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                if (input.state != 1 && input.roundId != "0")
                {
                    using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(documentBl))
                        {
                            DAL.Document betDocument = null;
                            if (input.type == 0)
                                betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.roundId, ProviderId, client.Id, (int)BetDocumentStates.Uncalculated);
                            else if (input.type == 1 && input.real > 0) //freebet
                            {
                                input.transactionId = $"{Constants.FreeSpinPrefix}{input.transactionId}";
                                var listOfOperationsFromApi = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    CurrencyId = client.CurrencyId,
                                    RoundId = input.roundId,
                                    ExternalProductId = product.ExternalId,
                                    GameProviderId = ProviderId,
                                    ProductId = product.Id,
                                    TransactionId = $"Bet_{input.transactionId}",
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0,
                                    DeviceTypeId = clientSession.DeviceType
                                });
                                betDocument = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);
                            }
                            if (input.type == 0 || input.real > 0)
                            {
                                if (betDocument == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                                var winDocument = documentBl.GetDocumentByExternalId(input.transactionId, client.Id, ProviderId,
                                                                                     partnerProductSetting.Id, (int)OperationTypes.Win);
                                if (winDocument == null)
                                {
                                    var state = input.real > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
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
                    }
                }
                releaseOutput.real = Math.Round(BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id), 2);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                releaseOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                releaseOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                releaseOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                releaseOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var output = CommonFunctions.ToXML(releaseOutput);
            WebApiApplication.DbLogger.Info("Output: " + output);
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(output, Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/playngo/cancelreserve")]
        public HttpResponseMessage CancelReserve(HttpRequestMessage httpRequestMessage)
        {
            var cancelReserveOutput = new Output.CancelReserve();
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("Input: " + inputString);
                var serializer = new XmlSerializer(typeof(Input.CancelReserve), new XmlRootAttribute("cancelReserve"));
                var input = (Input.CancelReserve)serializer.Deserialize(new StringReader(inputString));

                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.externalGameSessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var accessToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PlaynGoAccessToken);
                if (input.accessToken != accessToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
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
                            cancelReserveOutput.externalTransactionId = documentBl.RollbackProductTransactions(operationsFromProduct)[0].Id.ToString();
                        }
                        catch (FaultException<BllFnErrorType>)
                        {
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                    }
                }
                cancelReserveOutput.real = Math.Round(BaseHelpers.GetClientProductBalance(clientSession.Id, product.Id), 2);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                cancelReserveOutput.statusCode = PlaynGoHelpers.GetErrorCode(fex.Detail.Id);
                cancelReserveOutput.statusMessage = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(fex.Detail.Message);
            }
            catch (Exception ex)
            {
                cancelReserveOutput.statusCode = PlaynGoHelpers.GetErrorCode(Constants.Errors.GeneralException);
                cancelReserveOutput.statusMessage = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CommonFunctions.ToXML(cancelReserveOutput), Encoding.UTF8, Constants.HttpContentTypes.ApplicationXml)
            };
        }
    }
}