using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Racebook;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class RacebookController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Racebook).Id;
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "190.241.195.37",
            "181.193.33.229",
            "190.241.195.37",
            "181.193.33.229",
            "190.241.195.36",
            "190.241.195.38",
            "190.241.195.39",
            "190.241.195.40",
            "190.241.195.41",
            "190.241.195.42",
            "190.241.195.43",
            "190.241.195.44",
            "190.241.195.45",
            "190.241.195.46",
            "181.193.33.226",
            "181.193.33.228",
            "181.193.33.232",
            "181.193.33.230",
            "181.193.33.231",
            "181.193.33.233",
            "181.193.33.234",
            "181.193.33.235",
            "181.193.33.236",
            "181.193.33.237",
            "181.193.33.238"
        };

        [HttpPost]
        [Route("{partnerId}/api/Racebook/AuthenticateSession")]
        public ActionResult Authenticate(BaseInput input)
        {
            var output = new ClientOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(privateKey + input.Token);
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.ClientId = client.Id.ToString();
                output.Username = client.UserName;
                output.Currency = client.CurrencyId;
                output.Language = CommonHelpers.LanguageISO5646Codes[clientSession.LanguageId];
                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
                if (client.UserId.HasValue)
                {
                    var agent = CacheManager.GetUserById(client.UserId.Value);
                    output.Partner = agent.Id.ToString();
                    var agentTree = new List<int>();
                    var parentAgent = agent.Id;
                    var path = agent.Path.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    for (int i = 0; i < path.Count - 1; ++i)
                    {
                        var agentId = Convert.ToInt32(path[i]);
                        var child = CacheManager.GetUserById(agentId);
                        agentTree.Add(parentAgent);
                        parentAgent = agentId;
                    }
                    output.PartnersTree = String.Join(",", agentTree);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }

        [HttpPost]
        [Route("{partnerId}/api/Racebook/GetPlayerInformation")]
        public ActionResult GetClientData(BaseInput input)
        {
            var output = new ClientOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(privateKey + input.ClientId);
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.ClientId = client.Id.ToString();
                output.Username = client.UserName;
                output.Currency = client.CurrencyId;
                output.Language = "en-US";
                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
                if (client.UserId.HasValue)
                {
                    var agent = CacheManager.GetUserById(client.UserId.Value);
                    output.Partner = agent.Id.ToString();
                    var agentTree = new List<int>();
                    var parentAgent = agent.Id;
                    var path = agent.Path.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    for (int i = 0; i < path.Count - 1; ++i)
                    {
                        var agentId = Convert.ToInt32(path[i]);
                        var child = CacheManager.GetUserById(agentId);
                        agentTree.Add(parentAgent);
                        parentAgent = agentId;
                    }
                    output.PartnersTree = String.Join(",", agentTree);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }

        [HttpPost]
        [Route("{partnerId}/api/Racebook/GetPlayerBalance")]
        public ActionResult GetBalance(BaseInput input)
        {
            var output = new BaseOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(privateKey + input.ClientId);
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }


        [HttpPost]
        [Route("{partnerId}/api/Racebook/PlaceBet")]
        public ActionResult PlaceBet(BetInput input)
        {
            var output = new BaseOutput {TransactionId = "0" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.ExtraInfo, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null || clientSession.Id != client.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(string.Format("{0}{1}{2}{3}{4}{5}", privateKey, input.ClientId, input.Amount,
                                                                        input.BetId, input.TransactionId, input.ProductId));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(clientSession.LanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using var clientBl = new ClientBll(clientSession, Program.DbLogger);
                using var documentBl = new DocumentBll(clientBl);
                var document = documentBl.GetDocumentByExternalId(input.BetId, client.Id, ProviderId,
                                                                  partnerProductSetting.Id, (int)OperationTypes.Bet);
                if (document == null)
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        RoundId = input.BetId,
                        TransactionId = input.BetId,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = Convert.ToDecimal(input.Amount),
                        DeviceTypeId = clientSession.DeviceType
                    });
                    document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                }
                output.TransactionId = document.Id.ToString();
                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }

        [HttpPost]
        [Route("{partnerId}/api/Racebook/GradeBet")]
        public ActionResult DoWin(WinInput input)
        {
            var output = new BaseOutput { TransactionId = "0" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.ExtraInfo, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null || clientSession.Id != client.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(string.Format("{0}{1}{2}{3}{4}{5}{6}", privateKey, input.ClientId, input.BetId,
                                                                        input.TransactionId, input.ProductId, input.Amount, input.IsParcial.ToString().ToLower()));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(clientSession.LanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using var clientBl = new ClientBll(clientSession, Program.DbLogger);
                using var documentBl = new DocumentBll(clientBl);
                var betDocument = documentBl.GetDocumentByExternalId(input.BetId, client.Id, ProviderId,
                                                                   partnerProductSetting.Id, (int)OperationTypes.Bet);
                if (betDocument == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                var winTransactionId = string.Format("{0}_{1}", input.BetId, input.TransactionId);
                var winDonument = documentBl.GetDocumentByExternalId(winTransactionId, client.Id, ProviderId,
                                                                                       partnerProductSetting.Id, (int)OperationTypes.Win);
                if (winDonument == null)
                {
                    var amount = Convert.ToDecimal(input.Amount);
                    var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = betDocument.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = input.BetId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalOperationId = null,
                        ExternalProductId = product.ExternalId,
                        ProductId = betDocument.ProductId,
                        TransactionId = winTransactionId,
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount,
                        DeviceTypeId = betDocument.DeviceTypeId
                    });
                    var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                    if (amount > 0)
                    {
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    output.TransactionId = doc[0].Id.ToString();
                }
                else output.TransactionId = winDonument.Id.ToString();

                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }

        [HttpPost]
        [Route("{partnerId}/api/Racebook/RegradeBet")]
        public ActionResult Rollback(WinInput input)
        {
            var output = new BaseOutput { TransactionId = "0" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.ExtraInfo, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null || clientSession.Id != client.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(string.Format("{0}{1}{2}{3}{4}{5}", privateKey, input.ClientId, input.BetId,
                                                                        input.TransactionId, input.ProductId, input.Amount));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(clientSession.LanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                using var clientBl = new ClientBll(clientSession, Program.DbLogger);
                using var documentBl = new DocumentBll(clientBl);
                var betDocument = documentBl.GetDocumentByExternalId(input.BetId, client.Id, ProviderId,
                                                partnerProductSetting.Id, (int)OperationTypes.Bet);
                if (betDocument == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                var transactionId = "0";
                try
                {
                    var winDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Win, input.BetId, ProviderId, client.Id, null);
                    if (winDocuments != null && winDocuments.Any())
                    {
                        winDocuments.ForEach(doc =>
                        {
                            try
                            {
                                transactionId =  doc.Id.ToString();
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    GameProviderId = ProviderId,
                                    OperationTypeId = (int)OperationTypes.Win,
                                    TransactionId = doc.ExternalTransactionId
                                };
                                documentBl.RollbackProductTransactions(operationsFromProduct, false, string.Format("regrated_{0}", doc.ExternalTransactionId));
                            }
                            catch (FaultException<BllFnErrorType> fex)
                            {
                                if (fex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
                                    throw;
                            }
                        });
                    }
                    var amount = Convert.ToDecimal(input.Amount);
                    var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var winOperationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = betDocument.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = input.BetId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalOperationId = null,
                        ExternalProductId = product.ExternalId,
                        ProductId = betDocument.ProductId,
                        TransactionId = string.Format("{0}_{1}", input.BetId, input.TransactionId),
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    winOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount,
                        DeviceTypeId = betDocument.DeviceTypeId
                    });
                    var winDocument = clientBl.CreateDebitsToClients(winOperationsFromProduct, betDocument, documentBl);
                    if (amount > 0)
                    {
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    output.TransactionId = winDocument[0].Id.ToString();
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    if (fex.Detail.Id != (int)Constants.Errors.DocumentAlreadyRollbacked)
                        throw;
                    output.TransactionId = transactionId;
                }
                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }
        [HttpPost]
        [Route("{partnerId}/api/Racebook/Credit")]
        public ActionResult BonusWin(BetInput input)
        {
            var output = new BaseOutput { TransactionId = "0" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null )
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var privateKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.RacebookPrivateKey);
                var hash = CommonFunctions.ComputeSha256(string.Format("{0}{1}{2}{3}{4}", privateKey, input.ClientId, input.Amount,
                                                                        input.TransactionId, input.TransactionType));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, "RB");
                if (product == null)
                    throw BaseBll.CreateException(client.LanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var winDonument = documentBl.GetDocumentByExternalId("BonusWin_" + input.TransactionId, client.Id, ProviderId,
                                                                       partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDonument == null)
                    {
                        var betOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            RoundId = input.TransactionId,
                            State = (int)BetDocumentStates.Won,
                            TransactionId = "BonusBet_" + input.TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0m
                        });
                        var betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.TransactionId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            TransactionId = "BonusWin_" + input.TransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = Convert.ToDecimal(input.Amount),
                            DeviceTypeId = betDocument.DeviceTypeId
                        });
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = Convert.ToDecimal(input.Amount),
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });

                        output.TransactionId = doc[0].Id.ToString();
                    }
                    else output.TransactionId = winDonument.Id.ToString();
                    output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Error = fex.Detail.Id;
                output.ErrorDescription = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Error = Constants.Errors.GeneralException;
                output.ErrorDescription = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(output);
        }
    }
}