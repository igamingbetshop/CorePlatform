using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.Mancala;
using Newtonsoft.Json;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class MancalaController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Mancala).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "142.91.122.42",
            "142.91.122.44",
            "188.42.54.138",
            "188.42.54.147",
            "188.42.59.92",
            "188.42.59.76"
        };

        [HttpPost]
        [Route("{partnerId}/api/Mancala/Balance")]
        public ActionResult GetBalance(BaseInput input)
        {
            var response = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.ExtraData, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MancalaApiKey);

                var hash = CommonFunctions.ComputeMd5(string.Format("Balance/{0}{1}", input.SessionId, apiKey));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                response.Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Error = MancalaHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Msg = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                response.Error = MancalaHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Msg = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Mancala/Credit")]
        public ActionResult DoBet(BetInput input)
        {
            var response = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.ExtraData, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MancalaApiKey);

                var hash = CommonFunctions.ComputeMd5(string.Format("Credit/{0}{1}{2}{3}{4}", input.SessionId, input.TransactionGuid, input.RoundGuid,
                                                                                      input.Amount.ToString("0.####"), apiKey));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var document = documentBl.GetDocumentByExternalId(input.TransactionGuid, client.Id, ProviderId,
                                                                           partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundGuid,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.TransactionGuid,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }
                    }
                }
                response.Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Error = MancalaHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Msg = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                response.Error = MancalaHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Msg = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Mancala/Debit")]
        public ActionResult DoWin(BetInput input)
        {
            var response = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.ExtraData, Constants.DefaultLanguageId, null, false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MancalaApiKey);

                var hash = CommonFunctions.ComputeMd5(string.Format("Debit/{0}{1}{2}{3}{4}", input.SessionId, input.TransactionGuid, input.RoundGuid,
                                                                                      input.Amount.ToString("0.####"), apiKey));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundGuid, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionGuid, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var state = input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundGuid,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ProductId = product.Id,
                                TransactionId = input.TransactionGuid,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount,
                                DeviceTypeId = clientSession.DeviceType
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
                    }
                }
                response.Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Error = MancalaHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Msg = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                response.Error = MancalaHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Msg = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Mancala/Refund")]
        public ActionResult Rollback(RefuntInput input)
        {
            var response = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.ExtraData, Constants.DefaultLanguageId, null, false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.MancalaApiKey);

                var hash = CommonFunctions.ComputeMd5(string.Format("RefundId{0}{1}{2}{3}{4}{5}", input.SessionId, input.TransactionId, input.RefundTransactionGuid,
                                                                     input.RoundGuid, input.Amount.ToString("0.####"), apiKey));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            GameProviderId = ProviderId,
                            TransactionId = input.RefundTransactionGuid,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id
                        };
                        try
                        {
                            documentBl.RollbackProductTransactions(operationsFromProduct);
                        }
                        catch (FaultException<BllFnErrorType>)
                        {
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                    }
                }
                response.Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Error = MancalaHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Msg = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                response.Error = MancalaHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Msg = ex.Message;
            }
            return Ok(response);
        }
    }
}