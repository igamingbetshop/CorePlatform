using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.GoldenRace;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System;
using IqSoft.CP.Common.Enums;
using System.Text;
using IqSoft.CP.DAL.Models;
using System.Linq;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class GoldenRaceController : ControllerBase
    {

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.GoldenRace).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "",
        };


        [HttpPost]
        [Route("{partnerId}/api/GoldenRace/login")]
        [Route("{partnerId}/api/GoldenRace/logout")]
        [Route("{partnerId}/api/GoldenRace/balance")]
        public ActionResult CheckSession(HttpRequestMessage httpRequestMessage)
        {
            var baseOutput = new BaseOutput { Data = new DataModel() };
            var privateKey = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;// for log

            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(inputString);
                var input = JsonConvert.DeserializeObject<LoginInput>(inputString);
                var clientSession = ClientBll.GetClientProductSession(input.Action == "login" ? input.Token : input.SessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceApiKey);
                var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceSiteId);
                var fingerprint = GenerateFingerprint(inputString, input.Timestamp, privateKey);
                if (fingerprint.ToLower() != input.Fingerprint.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.SiteId != siteId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    baseOutput.Data = new DataModel
                    {
                        ClientId = client.Id.ToString(),
                        UserName =input.Action == "login" ? client.UserName : String.Empty,
                        Currency = client.CurrencyId,
                        Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance,
                        SessionId = input.Action == "login" ? input.Token : input.SessionId,
                        Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                        RequestId = input.RequestId
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                baseOutput.Message = fex.Detail.Message;
                baseOutput.Status = false;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                baseOutput.Status = false;
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            if (string.IsNullOrEmpty(privateKey))
                privateKey = CacheManager.GetGameProviderValueByKey(1, ProviderId, Constants.PartnerKeys.GoldenRaceApiKey);
            baseOutput.Data.Fingerprint = CommonFunctions.ComputeMd5(string.Format("{0}{1}", GetFingerprint(baseOutput.Data), privateKey));
            return Ok(baseOutput);
        }

        [HttpPost]
        [Route("{partnerId}/api/GoldenRace/debit")]
        public ActionResult DoBet(HttpRequestMessage httpRequestMessage)
        {
            var baseOutput = new BaseOutput { Data = new DataModel() };
            var privateKey = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(inputString);
                var input = JsonConvert.DeserializeObject<TransactoinInput>(inputString);
                var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceApiKey);
                var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceSiteId);
                var fingerprint = GenerateFingerprint(inputString, input.Timestamp, privateKey);
                if (fingerprint.ToLower() != input.Fingerprint.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.SiteId != siteId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (input.TransactionCategory != "bet")
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null || clientSession.ProductId != product.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                var oldBalance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;
                string transactionId;

                using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                        var betByRoundId = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id);
                        if (betByRoundId != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongDocumentNumber);

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
                            Amount = input.TransactionAmount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        transactionId = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl).Id.ToString();
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                }
                baseOutput.Data = new DataModel
                {
                    ClientId = client.Id.ToString(),
                    Currency = client.CurrencyId,
                    Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance,
                    OldBalance = oldBalance,                   
                    TransactionId  = transactionId,
                    SessionId = input.SessionId,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                    RequestId = input.RequestId
                };   
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                baseOutput.Message = fex.Detail.Message;
                baseOutput.Status = false;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                baseOutput.Status = false;
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            Program.DbLogger.Info("fingerprint: " + GetFingerprint(baseOutput.Data));
            baseOutput.Data.Fingerprint = CommonFunctions.ComputeMd5(string.Format("{0}{1}", GetFingerprint(baseOutput.Data), privateKey));
            return Ok(baseOutput);
        }

        [HttpPost]
        [Route("{partnerId}/api/GoldenRace/credit")]
        public ActionResult DoWin(HttpRequestMessage httpRequestMessage)
        {
            var baseOutput = new BaseOutput { Data = new DataModel() };
            var privateKey = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(inputString);
                var input = JsonConvert.DeserializeObject<TransactoinInput>(inputString);
                var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceApiKey);
                var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceSiteId);
                var fingerprint = GenerateFingerprint(inputString, input.Timestamp, privateKey);
                if (fingerprint.ToLower() != input.Fingerprint.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.SiteId != siteId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (input.TransactionCategory != "win" && input.TransactionCategory != "jackpotwin")
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null || clientSession.ProductId != product.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                var oldBalance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;
                string transactionId;
                using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RoundNotFound);

                        var winByRound = documentBl.GetDocumentByRoundId((int)OperationTypes.Win, input.RoundId + "_closed", ProviderId, client.Id);
                        if (winByRound != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WinAlreadyPayed);

                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var state = input.TransactionAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            if (input.GameCycleClosed)
                                input.RoundId += "_closed";
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
                                Info = input.TransactionCategory,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.TransactionAmount,
                                DeviceTypeId = clientSession.DeviceType
                            });

                            transactionId = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0].Id.ToString();
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = input.TransactionAmount,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        else
                            transactionId = winDocument.Id.ToString();
                    }
                }
                baseOutput.Data = new DataModel
                {
                    ClientId = client.Id.ToString(),
                    Currency = client.CurrencyId,
                    OldBalance = oldBalance,
                    Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance,
                    TransactionId  = transactionId,
                    SessionId = input.SessionId,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                    RequestId = input.RequestId
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                baseOutput.Message = fex.Detail.Message;
                baseOutput.Status = false;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                baseOutput.Status = false;
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            baseOutput.Data.Fingerprint = CommonFunctions.ComputeMd5(string.Format("{0}{1}", GetFingerprint(baseOutput.Data), privateKey));
            return Ok(baseOutput);
        }

        [HttpPost]
        [Route("{partnerId}/api/GoldenRace/rollback")]
        public ActionResult Rollback(HttpRequestMessage httpRequestMessage)
        {
            var baseOutput = new BaseOutput { Data = new DataModel() };
            var privateKey = string.Empty;
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(inputString);
                var input = JsonConvert.DeserializeObject<TransactoinInput>(inputString);
                var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceApiKey);
                var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GoldenRaceSiteId);
                var fingerprint = GenerateFingerprint(inputString, input.Timestamp, privateKey);
                if (fingerprint.ToLower() != input.Fingerprint.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.SiteId != siteId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null || clientSession.ProductId != product.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                var oldBalance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;
                var transactionId = string.Empty;
                using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var transactionType = input.TransactionType == "debit" ? (int)OperationTypes.Bet : (int)OperationTypes.Win;
                        var document = documentBl.GetDocumentByRoundId(transactionType, input.RoundId, ProviderId, client.Id);
                        if (document==null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RoundNotFound);
                        var winByRound = documentBl.GetDocumentByRoundId((int)OperationTypes.Win, input.RoundId + "_closed", ProviderId, client.Id);
                        if (winByRound != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WinAlreadyPayed);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            GameProviderId = ProviderId,
                            TransactionId = input.TransactionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id
                        };
                        transactionId = documentBl.RollbackProductTransactions(operationsFromProduct)[0].Id.ToString();
                        BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                    }
                }
                baseOutput.Data = new DataModel
                {
                    ClientId = client.Id.ToString(),
                    Currency = client.CurrencyId,
                    OldBalance = oldBalance,
                    Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance,
                    TransactionId  = transactionId,
                    SessionId = input.SessionId,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    RequestId = input.RequestId
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                baseOutput.Message = fex.Detail.Message;
                baseOutput.Status = false;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                baseOutput.Status = false;
                baseOutput.Code = GoldenRaceHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            Program.DbLogger.Info("RollbackFingerprint: " + GetFingerprint(baseOutput.Data));
            baseOutput.Data.Fingerprint = CommonFunctions.ComputeMd5(string.Format("{0}{1}", GetFingerprint(baseOutput.Data), privateKey));
            return Ok(baseOutput);
        }

        private static string GetFingerprint(object input)
        {
            var properties = from p in input.GetType().GetProperties()
                             select p.GetValue(input, null) != null ? p.GetValue(input, null).ToString() : string.Empty;
            return string.Join(string.Empty, properties);
        }

        private static string GenerateFingerprint(string jsonData, string timestamp, string privateKey)
        {
            var obj = JObject.Parse(jsonData);
            obj["fingerprint"] = string.Empty;
            obj["timestamp"] = timestamp;
            var inputValues = obj.Values().Select(x => x.ToString(Formatting.None)).ToList();
            var fingerprint = string.Join(string.Empty, inputValues) + privateKey;
            return CommonFunctions.ComputeMd5(fingerprint.Replace("\"", string.Empty));
        }
    }
}