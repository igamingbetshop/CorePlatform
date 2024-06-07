using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.Products;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.VisionaryiGaming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class VisionaryiGamingController : ApiController
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.VisionaryiGaming);
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.VisionaryiGaming);
        [HttpPost]
        [Route("{partnerId}/api/VisionaryiGaming/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage, int partnerId)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info(inputString);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.VisionarySecretKey);
            var inputData = inputString.Substring(inputString.IndexOf(" ")).Trim();
            var signature = inputString.Split(' ')[0];
            var sign = CommonFunctions.ComputeSha1(secretKey + inputData);
            var input = JsonConvert.DeserializeObject<BaseInput>(inputString.Replace(signature, string.Empty));
            var output = new AuthOutput();
            object response;
            try
            {
                if (signature != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.Method == VisionaryiGamingHelpers.Methods.Authenticate)
                {
                    var authOutputList = new List<AuthOutput>();
                    foreach (var arg in input.ArgumentList)
                    {
                        var clientSession = ClientBll.GetClientProductSession(arg.OTP, Constants.DefaultLanguageId);
                        if (clientSession == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);

                        var client = CacheManager.GetClientById(clientSession.Id);
                        var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.VisionarySiteId);
                        if (arg.SiteID != siteId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                        {
                            clientBl.RefreshClientSession(arg.OTP, true);
                        }
                        var authOutput = product.ExternalId == "lobby" ? new AuthOutput
                        {
                            Balance = BaseHelpers.GetClientProductBalance(clientSession.Id, clientSession.ProductId),
                            Status = "OK",
                            UserName = client.Id.ToString(),
                            ScreenName = client.UserName,
                            Currency = client.CurrencyId,
                            SiteID = siteId,
                            Flag = "onewallet lobby",
                        } :
                        new AuthOutput
                        {
                            Balance = BaseHelpers.GetClientProductBalance(clientSession.Id, clientSession.ProductId),
                            Status = "OK",
                            UserName = client.Id.ToString(),
                            ScreenName = client.UserName,
                            Currency = client.CurrencyId,
                            SiteID = siteId,
                            Flag = "onewallet game",
                            Game = product.NickName,
                            Table = "1",
                            Limitname = "1"
                        };

                        authOutputList.Add(authOutput);
                    }
                    response = new AuthenticateOutput { AuthenticateResponse = authOutputList };
                }
                else
                {
                    var balanceList = new List<BalanceOutput>();
                    BalanceOutput result;
                    switch (input.Method)
                    {
                        case VisionaryiGamingHelpers.Methods.BatchGetBalance:
                            foreach (var arg in input.ArgumentList)
                            {
                                try
                                {
                                    var client = CacheManager.GetClientById(Convert.ToInt32(arg.Username));
                                    if (client == null)
                                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                    var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.VisionarySiteId);
                                    if (arg.SiteID != siteId)
                                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);

                                    var sessionProduct = CacheManager.GetProductByExternalId(Provider.Id, "lobby");
                                    var clientSession = ClientBll.GetClientSessionByProductId(client.Id, sessionProduct.Id);

                                    result = new BalanceOutput
                                    {
                                        Balance = CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance
                                    };
                                }
                                catch (FaultException<BllFnErrorType> fex)
                                {
                                    WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(arg) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                                    result = new BalanceOutput
                                    {
                                        Status = "Error",
                                        Description = VisionaryiGamingHelpers.ErrorMapping(fex.Detail.Id, fex.Detail.Message)
                                    };
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(arg) + "_" + ex.Message);
                                    result = new BalanceOutput
                                    {
                                        Status = "Error",
                                        Description = ex.Message
                                    };
                                }
                                balanceList.Add(result);
                            }
                            response = new BatchGetBalanceOutput() { BatchGetBalanceResponse = balanceList };  //??
                            break;
                        case VisionaryiGamingHelpers.Methods.BatchDebitFunds:
                            foreach (var arg in input.ArgumentList)
                            {
                                balanceList.Add(DoBet(arg));
                            }
                            response = new BatchDebitFundsOutput() { BatchDebitFundsResponse = balanceList };
                            break;
                        case VisionaryiGamingHelpers.Methods.BatchCreditFunds:
                            foreach (var arg in input.ArgumentList)
                            {
                                balanceList.Add(DoWin(arg));
                            }
                            response = new BatchCreditFundsOutput() { BatchCreditFundsResponse = balanceList };
                            break;

                        default:
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(inputString) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                output.Status = "Error";
                output.Description = VisionaryiGamingHelpers.ErrorMapping(fex.Detail.Id, fex.Detail.Message);
                response = VisionaryiGamingHelpers.ErrorResponce(input.Method, output);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(inputString) + "_" + ex.Message);
                output.Status = "Error";
                output.Description = ex.Message;
                response = VisionaryiGamingHelpers.ErrorResponce(input.Method, output);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }

        private BalanceOutput DoBet(Argument input)
        {
            BalanceOutput result;
            try
            {
                var client = CacheManager.GetClientById(Convert.ToInt32(input.Username));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var product = CacheManager.GetProductByExternalId(Provider.Id, input.TableID.Split('|')[0]);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var sessionProduct = CacheManager.GetProductByExternalId(Provider.Id, "lobby");
                var clientSession = ClientBll.GetClientSessionByProductId(client.Id, sessionProduct.Id);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransferID, client.Id, Provider.Id,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.Id,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = Provider.Id,
                            RoundId = input.TransferID,
                            ExternalProductId = input.GameID,
                            ProductId = product.Id,
                            TransactionId = input.TransferID,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount,
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        betDocument.State = (int)BetDocumentStates.Lost;
                        var recOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.Id,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = Provider.Id,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = input.GameID,
                            ProductId = product.Id,
                            TransactionId = input.TransferID + "_win",
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Lost,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        recOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var doc = clientBl.CreateDebitsToClients(recOperationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        result = new BalanceOutput { Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                result = new BalanceOutput
                {
                    Status = "Error",
                    Description = VisionaryiGamingHelpers.ErrorMapping(fex.Detail.Id, fex.Detail.Message)
                };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                result = new BalanceOutput
                {
                    Status = "Error",
                    Description = ex.Message
                };
            }
            return result;
        }

        private BalanceOutput DoWin(Argument input)
        {
            var client = CacheManager.GetClientById(Convert.ToInt32(input.Username));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            if (input.Currency != client.CurrencyId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductByExternalId(Provider.Id, input.TableID.Split('|')[0]);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var winDocument = documentBl.GetDocumentByExternalId(input.TransferID, client.Id, Provider.Id,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null && input.Amount > 0)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = Provider.Id,
                            ProductId = product.Id,
                            TransactionId = input.TransferID + "_bet",
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        betDocument.State = (int)BetDocumentStates.Won;
                        operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = Provider.Id,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = input.GameID,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.TransferID,
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount
                        });
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
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
                    else if (winDocument == null)
                        return new BalanceOutput { Description = "Transaction Reused", Status = "Error" };

                    return new BalanceOutput { Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id), Status = "OK", Description = "OK" };
                }
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/VisionaryiGaming/Games")]
        public HttpResponseMessage Games(HttpRequestMessage httpRequestMessage, int partnerId)
        {
            var response = new GameOutput();
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            var signature = inputString.Split(' ')[0];
            var input = JsonConvert.DeserializeObject<GameInput>(inputString.Replace(signature, string.Empty));
            if (input.Method == VisionaryiGamingHelpers.Methods.LobbyStatus || input.Method == VisionaryiGamingHelpers.Methods.RNGLobbyStatus)
            {
                try
                {
                    var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.VisionarySecretKey);
                    var inputData = inputString.Substring(inputString.IndexOf(" ")).Trim();
                    var sign = CommonFunctions.ComputeSha1(secretKey + inputData);
                    if (signature != sign)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var gameList = new List<fnProduct>();
                    using (var productBll = new ProductBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {

                        var parentId = productBll.GetProducts(new FilterProduct() { Description = Provider.Name }, false).FirstOrDefault(x => x.GameProviderId == null).Id;
                        switch (input.Method)
                        {
                            case VisionaryiGamingHelpers.Methods.LobbyStatus:
                                var lobbyStatus = JsonConvert.DeserializeObject<List<LobbyArgumentList>>(JsonConvert.SerializeObject(input.ArgumentList));
                                var lobbytables = lobbyStatus.Select(x => x.Tables).FirstOrDefault();
                                var groupedlobby = lobbytables.GroupBy(x => x.TableID).ToList();
                                foreach (var lobbyTable in groupedlobby)
                                {
                                    var product = new fnProduct
                                    {
                                        GameProviderId = Provider.Id,
                                        ParentId = parentId,
                                        NickName = lobbyTable.FirstOrDefault().Game,
                                        SubproviderId = Provider.Id,
                                        Name = lobbyTable.FirstOrDefault().Game,
                                        ExternalId = lobbyTable.FirstOrDefault().TableID,
                                        State = (int)ProductStates.Active,
                                        WebImageUrl = lobbyTable.FirstOrDefault().Dealer.PhotoURL,
                                        MobileImageUrl = lobbyTable.FirstOrDefault().Dealer.PhotoURL,
                                        IsForDesktop = true,
                                        IsForMobile = true
                                    };
                                    gameList.Add(product);
                                }
                                break;
                            case VisionaryiGamingHelpers.Methods.RNGLobbyStatus:
                                var rngLobbyStatus = JsonConvert.DeserializeObject<List<RNGArgumentList>>(JsonConvert.SerializeObject(input.ArgumentList));
                                foreach (var rngTable in rngLobbyStatus.Select(x => x.Tables).FirstOrDefault())
                                {
                                    var product = new fnProduct
                                    {
                                        GameProviderId = Provider.Id,
                                        ParentId = parentId,
                                        NickName = rngTable.Name,
                                        SubproviderId = Provider.Id,
                                        Name = rngTable.Name,
                                        ExternalId = rngTable.GameId,
                                        State = (int)ProductStates.Active,
                                        WebImageUrl = rngLobbyStatus.FirstOrDefault().BaseURL + rngTable.Icon,
                                        MobileImageUrl = rngLobbyStatus.FirstOrDefault().BaseURL + rngTable.Icon
                                    };
                                    gameList.Add(product);
                                }
                                break;
                        };
                        if (gameList.Any())
                        {
                            var ids = productBll.SynchronizeProducts(Provider.Id, gameList);
                            productBll.SavePartnerProductSettings(new ApiPartnerProductSettingInput { ProductIds = ids, PartnerId = partnerId, CategoryIds = new List<int>() }, false);
                            CacheManager.RemoveFromCache(string.Format("{0}_{1}", Constants.CacheItems.ClientProductCategories, partnerId));
                            foreach (var id in ids)
                            {
                                CacheManager.DeleteProductFromCache(id);
                                CacheManager.RemovePartnerProductSetting(partnerId, id);
                            }
                        }
                    }
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(inputString) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                    response = new GameOutput() { Status = "Error", Description = fex.Detail.Message };
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(inputString) + "_" + ex.Message);
                    response = new GameOutput() { Status = "Error", Description = ex.Message };
                }
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }

    }
}