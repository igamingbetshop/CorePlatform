using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Mahjong;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class MahjongController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Mahjong).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Mahjong);
        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.Euro,
            Constants.Currencies.JapaneseYen
        };

        [HttpPost]
        [Route("{partnerId}/api/Mahjong/login")]
        public HttpResponseMessage CheckSession(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync();
            var loginOutput = new LoginOutput() { LoginResponse = new LoginResponse() };
            try
            {
                WebApiApplication.DbLogger.Info("inputString " + inputString.Result);
                BaseBll.CheckIp(WhitelistedIps);
                var serializer = new XmlSerializer(typeof(LoginInput), new XmlRootAttribute("login"));
                var input = (LoginInput)serializer.Deserialize(new StringReader(inputString.Result));
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MahjongSecretKey);
                if (input.SecretKey != secretKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var partnerId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MahjongPartnerId);
                loginOutput.LoginResponse = new LoginResponse
                {
                    User_id = client.Id.ToString(),
                    Username = client.UserName,
                    Currency = client.CurrencyId,
                    Partner_id = partnerId,
                    K_y_c = true
                };
                loginOutput.Echo = input.Echo;
                loginOutput.ErrorCode = "100";
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + ex.Message);
            }

            var response = SerializeAndDeserialize.SerializeToXml(loginOutput, "login_response");
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8, Constants.HttpContentTypes.TextXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/Mahjong/balance")]
        public HttpResponseMessage GetBalanse(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync();
            var balanceOutput = new BalanceOutput() { BalanceResponse = new BalanceResponse() };

            try
            {
                WebApiApplication.DbLogger.Info("inputString " + inputString.Result);
                var serializer = new XmlSerializer(typeof(BalanceInput), new XmlRootAttribute("balance"));
                var input = (BalanceInput)serializer.Deserialize(new StringReader(inputString.Result));
                var client = CacheManager.GetClientById(Convert.ToInt32(input.UserId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MahjongSecretKey);
                if (input.SecretKey != secretKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                
                var balance = BaseHelpers.GetClientProductBalance(client.Id, 0) * 100;
                if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                balanceOutput.BalanceResponse = new BalanceResponse
                {
                    Amount = Math.Floor(balance).ToString(),
                    Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                };
                balanceOutput.Echo = input.Echo;
                balanceOutput.ErrorCode = "100";
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + ex.Message);
            }

            var response = SerializeAndDeserialize.SerializeToXml(balanceOutput, "balance_response");
            WebApiApplication.DbLogger.Info("balanca  " + response);
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8, Constants.HttpContentTypes.TextXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/Mahjong/transfer")]
        public HttpResponseMessage Transfer(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync();
            var transferOutput = new TransferOutput() { TransferResponse = new TransferResponse() };

            try
            {
                WebApiApplication.DbLogger.Info("inputString " + inputString.Result);
                var serializer = new XmlSerializer(typeof(TransferInput), new XmlRootAttribute("transfer"));
                var input = (TransferInput)serializer.Deserialize(new StringReader(inputString.Result));
                var checkExpiration = input.Direction == "to_mahjong";
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null, checkExpiration);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MahjongSecretKey);
                if (input.SecretKey != secretKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                var documentId = string.Empty;
                if (input.Direction == "to_mahjong")
                {
                    documentId = DoBet(input, client, clientSession, partnerProductSetting.Id, product);
                }
                if (input.Direction == "from_mahjong")
                {
                    documentId = DoWin(input, client, clientSession, partnerProductSetting.Id, product);
                }
                transferOutput.TransferResponse = new TransferResponse
                {
                    UserId = client.Id.ToString(),
                    Amount = input.Amount,
                    Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                    Direction = input.Direction,
                    MahjongTransactionId = input.MahjongTransactionId,
                    PartnerTransactionId = documentId
                };
                transferOutput.Echo = input.Echo;
                transferOutput.ErrorCode = "100";
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                transferOutput.TransferResponse.ErrorReason = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + ex.Message);
                transferOutput.TransferResponse.ErrorReason = ex.Message;
            }

            var response = SerializeAndDeserialize.SerializeToXml(transferOutput, "transfer_response");
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8, Constants.HttpContentTypes.TextXml)
            };
        }

        private string DoBet(TransferInput input, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {                
                    var document = documentBl.GetDocumentByExternalId(input.GameId, clientSession.Id, ProviderId,
                                                                            partnerProductSettingId, (int)OperationTypes.Bet);

                    var amount = Convert.ToDecimal(input.Amount);
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    {
                        amount = Convert.ToDecimal(BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount));
                    }
                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.Token,
                            ExternalProductId = input.GameId,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
                            TransactionId = input.GameId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount / 100,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    return document.Id.ToString();
                }
            }
        }

        private string DoWin(TransferInput input, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var betDocument = documentBl.GetDocumentByExternalId(input.GameId, client.Id,
                           ProviderId, partnerProductSettingId, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    var winDocument = documentBl.GetDocumentByExternalId(input.GameId, clientSession.Id, ProviderId,
                                                                                partnerProductSettingId, (int)OperationTypes.Win);
                    if (winDocument != null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentAlreadyWinned);

                    var amount = Convert.ToDecimal(input.Amount);
                    if (winDocument == null)
                    {
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                        {
                            amount = Convert.ToDecimal(BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount));
                        }
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            RoundId = input.Token,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.GameId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount / 100,
                            PossibleWin = input.Rake?.Amount == null ? 0 : (decimal)input.Rake.Amount / 100
                        });
                        winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];

                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = amount / 100,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    return winDocument.Id.ToString();
                }
            }
        }
    }
}