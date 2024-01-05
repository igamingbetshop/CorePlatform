using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.ProductGateway.Models.BetSolutions;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.ServiceModel;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class BetSolutionsController : ControllerBase
    {
        private readonly static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetSolutions).Id;
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "94.130.225.50"
        };

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/Authentication")]
        public ActionResult Authentication(AuthenticationInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.PublicToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", input.Hash, key));
                //if (hash.ToLower() != input.Hash.ToLower())
                //    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    response.Data = new { PrivateToken = clientBl.RefreshClientSession(input.PublicToken, true).Token };
                }


            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/GetBalance")]
        public ActionResult GetBalance(BaseInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", CommonFunctions.GetSortedValuesAsString(input, "|"), key));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                response.Data = new { CurrentBalance = Math.Truncate(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100) };

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/Bet")]
        public ActionResult DoBet(BetInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", CommonFunctions.GetSortedValuesAsString(input, "|"), key));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (document == null)
                        {
                            if (input.BetTypeId == (int)BetSolutionsHelpers.TransactionsTypes.Normal)
                            {
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    CurrencyId = client.CurrencyId,
                                    GameProviderId = ProviderId,
                                    ProductId = product.Id,
                                    RoundId = input.RoundId,
                                    TransactionId = input.TransactionId,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = input.Amount / 100m,
                                    DeviceTypeId = clientSession.DeviceType
                                });
                                document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                                response.Data = new
                                {
                                    CurrentBalance = Math.Truncate(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100),
                                    TransactionId = document.Id
                                };

                            }
                        }
                        response.Data = new { CurrentBalance = Math.Truncate(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100) };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/Win")]
        public ActionResult DoWin(BetInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", CommonFunctions.GetSortedValuesAsString(input, "|"), key));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        if (betDocument.ProductId != product.Id)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongProductId);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null && input.WinTypeId == (int)BetSolutionsHelpers.TransactionsTypes.Normal)
                        {
                            var amount = input.Amount / 100M;
                            var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ExternalProductId = input.GameId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.TransactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
                            });

                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            response.Data = new
                            {
                                CurrentBalance = Math.Truncate(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100),
                                TransactionId = winDocument.Id
                            };
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = Convert.ToDecimal(amount),
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
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            return Ok(response);
        }
    }
}