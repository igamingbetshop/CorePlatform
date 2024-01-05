using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.VisionaryiGaming;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class VisionaryiGamingController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.VisionaryiGaming).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "",
        };

        [HttpPost]
        [Route("{partnerId}/api/VisionaryiGaming/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage httpRequestMessage, int partnerId)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            Program.DbLogger.Info(JsonConvert.SerializeObject(inputString));
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.VisionarySecretKey);
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
                        var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.VisionarySiteId);
                        if (arg.SiteID != siteId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                        clientBl.RefreshClientSession(arg.OTP, true);
                        var authOutput = product.ExternalId == "lobby" ? new AuthOutput
                        {
                            Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance,
                            Status = "OK",
                            UserName = client.Id.ToString(),
                            ScreenName = client.UserName,
                            Currency = client.CurrencyId,
                            SiteID = siteId,
                            Flag = "onewallet lobby",
                        } :
                        new AuthOutput
                        {
                            Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance,
                            Status = "OK",
                            UserName = client.Id.ToString(),
                            ScreenName = client.UserName,
                            Currency = client.CurrencyId,
                            SiteID = siteId,
                            Flag = "onewallet game",
                            Game = product.NickName,
                            Table = "1"
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
                                    var sessionProduct = CacheManager.GetProductByExternalId(ProviderId, "lobby");
                                    var clientSession = ClientBll.GetClientSessionByProductId(client.Id, sessionProduct.Id);
                                    var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.VisionarySiteId);
                                    if (arg.SiteID != siteId)
                                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
                                    result = new BalanceOutput
                                    {
                                        Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance
                                    };
                                }
                                catch (FaultException<BllFnErrorType> fex)
                                {
                                    Program.DbLogger.Error(JsonConvert.SerializeObject(arg) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                                    result = new BalanceOutput
                                    {
                                        Status = "Error",
                                        Description = VisionaryiGamingHelpers.ErrorMapping(fex.Detail.Id, fex.Detail.Message)
                                    };
                                }
                                catch (Exception ex)
                                {
                                    Program.DbLogger.Error(JsonConvert.SerializeObject(arg) + "_" + ex.Message);
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
                return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(inputString) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                output.Status = "Error";
                output.Description = VisionaryiGamingHelpers.ErrorMapping(fex.Detail.Id, fex.Detail.Message);
                response = VisionaryiGamingHelpers.ErrorResponce(input.Method, output);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(inputString) + "_" + ex.Message);
                output.Status = "Error";
                output.Description = ex.Message;
                response = VisionaryiGamingHelpers.ErrorResponce(input.Method, output);
                return BadRequest(response);
            }
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
                var product = CacheManager.GetProductByExternalId(ProviderId, input.TableID);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var sessionProduct = CacheManager.GetProductByExternalId(ProviderId, "lobby");
                var clientSession = ClientBll.GetClientSessionByProductId(client.Id, sessionProduct.Id);
                using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                using var documentBl = new DocumentBll(clientBl);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                var betDocument = documentBl.GetDocumentByExternalId(input.TransferID, client.Id, ProviderId,
                                                                  partnerProductSetting.Id, (int)OperationTypes.Bet);
                if (betDocument != null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = clientSession.Id,
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
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
                betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                betDocument.State = (int)BetDocumentStates.Lost;
                var recOperationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = clientSession.Id,
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
                    OperationTypeId = (int)OperationTypes.Win,
                    ExternalOperationId = null,
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
                result = new BalanceOutput { Balance = CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                result = new BalanceOutput
                {
                    Status = "Error",
                    Description = VisionaryiGamingHelpers.ErrorMapping(fex.Detail.Id, fex.Detail.Message)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
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
            using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(documentBl);
            var product = CacheManager.GetProductByExternalId(ProviderId, input.TableID);
            if (product == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

            var winDocument = documentBl.GetDocumentByExternalId(input.TransferID, client.Id, ProviderId,
                                                                 partnerProductSetting.Id, (int)OperationTypes.Win);

            if (winDocument == null && input.Amount > 0)
            {
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
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
                var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                betDocument.State = (int)BetDocumentStates.Won;
                operationsFromProduct = new ListOfOperationsFromApi
                {
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
                    OperationTypeId = (int)OperationTypes.Win,
                    ExternalOperationId = null,
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

            return new BalanceOutput { Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, Status = "OK", Description = "OK" };
        }
    }
}