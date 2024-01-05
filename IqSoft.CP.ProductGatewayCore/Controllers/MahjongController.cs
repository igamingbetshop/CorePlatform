using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Mahjong;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class MahjongController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Mahjong).Id;
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "109.74.5.108",
            "83.218.20.242",
            "83.218.20.243",
            "83.218.20.244",
            "83.218.20.245",
            "83.218.20.246"
        };
        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.Euro,
            Constants.Currencies.JapaneseYen
        };

        [HttpPost]
        [Route("{partnerId}/api/Mahjong/login")]
        public ActionResult CheckSession(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync();
            var loginOutput = new LoginOutput() { LoginResponse = new LoginResponse() };
            try
            {
                Program.DbLogger.Info("inputString " + inputString.Result);
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
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
                response = SerializeAndDeserialize.SerializeToXml(loginOutput, "login_response");
                return Ok(new StringContent(response, Encoding.UTF8, Constants.HttpContentTypes.TextXml));

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                    response = "OK";
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.Message; ;
                Program.DbLogger.Error(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/Mahjong/balance")]
        public ActionResult GetBalanse(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync();
            var balanceOutput = new BalanceOutput() { BalanceResponse = new BalanceResponse() };

            try
            {
                Program.DbLogger.Info("inputString " + inputString.Result);
                var serializer = new XmlSerializer(typeof(BalanceInput), new XmlRootAttribute("balance"));
                var input = (BalanceInput)serializer.Deserialize(new StringReader(inputString.Result));
                var client = CacheManager.GetClientById(Convert.ToInt32(input.UserId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MahjongSecretKey);
                if (input.SecretKey != secretKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);

                var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100;
                if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                balanceOutput.BalanceResponse = new BalanceResponse
                {
                    Amount = Math.Floor(balance).ToString(),
                    Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                };
                balanceOutput.Echo = input.Echo;
                balanceOutput.ErrorCode = "100";
                response = SerializeAndDeserialize.SerializeToXml(balanceOutput, "balance_response");
                return Ok(new StringContent(response, Encoding.UTF8, Constants.HttpContentTypes.TextXml));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                    response = "OK";
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.Message; ;
                Program.DbLogger.Error(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/Mahjong/transfer")]
        public ActionResult Transfer(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync();
            var transferOutput = new TransferOutput() { TransferResponse = new TransferResponse() };

            try
            {
                Program.DbLogger.Info("inputString " + inputString.Result);
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
                response = SerializeAndDeserialize.SerializeToXml(transferOutput, "transfer_response");
                return Ok(new StringContent(response, Encoding.UTF8, Constants.HttpContentTypes.TextXml));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                transferOutput.TransferResponse.ErrorReason = fex.Detail.Message;
                response = SerializeAndDeserialize.SerializeToXml(transferOutput, "transfer_response");
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                transferOutput.TransferResponse.ErrorReason = ex.Message;
                response = SerializeAndDeserialize.SerializeToXml(transferOutput, "transfer_response");
                return BadRequest(response);
            }
        }

        private string DoBet(TransferInput input, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product)
        {
            using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(documentBl);
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
                document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
            }
            return document.Id.ToString();
        }

        private string DoWin(TransferInput input, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product)
        {
            using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(documentBl);
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