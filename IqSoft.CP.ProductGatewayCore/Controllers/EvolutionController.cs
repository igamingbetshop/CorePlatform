using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Evolution;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Cache;
using System.Net.Http;
using System.Text;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Integration.Platforms.Helpers;

using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class EvolutionController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
			"35.157.123.250",
            "91.213.212.38",
            "91.213.212.254",
            "87.246.163.*"
		};

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Evolution).Id;
        [HttpPost]
        [Route("{partnerId}/api/Evolution/Check")]
        public ActionResult CheckSession([FromQuery] string authToken, [FromBody] CheckInput request)
        {
            var response = new CheckOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                    var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(request.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var partnerAuthToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvolutionAuthToken);
                    if (partnerAuthToken != authToken)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    if (clientSession.Id != request.ClientId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongParameters);
                    response.Sid = clientBl.RefreshClientSession(clientSession.Token, true).Token;
                    BaseHelpers.RemoveSessionFromeCache(clientSession.Token, null);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                }
                else
                {
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                }
                Program.DbLogger.Error("Error: " + fex.Detail == null ? string.Empty : JsonConvert.SerializeObject(fex.Detail) + "Description: " + fex.Message);
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                Program.DbLogger.Error("Error: " + JsonConvert.SerializeObject(response) + "Description: " + ex.Message);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/Balance")]
        public ActionResult Balance([FromQuery] string authToken, [FromBody] BalanceInput request)
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response.Balance = GetBalance(client, request.Sid, request.ClientId, authToken, true);
                response.Bonus = 0;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                }
                else
                {
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                }
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + fex.Message);
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) +"Error: response " + JsonConvert.SerializeObject(response) + "Description: " + ex);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/debit")]
        public ActionResult Credit([FromQuery] string authToken, [FromBody] FinOperationInput request)// place bet
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId);
                        var client = CacheManager.GetClientById(request.ClientId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        var partnerAuthToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvolutionAuthToken);
                        if (partnerAuthToken != authToken)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                        if (client.Id != clientSession.Id)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                        var product = CacheManager.GetProductByExternalId(ProviderId, request.Game.Details.Table.Id);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var document = documentBl.GetDocumentByExternalId(request.Transaction.RefId, client.Id,
                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document != null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.TransactionAlreadyExists);

                        var currency = client.CurrencyId;
                        if (EvolutionHelpers.RestrictedCurrencies.Contains(client.CurrencyId))
                        {
                            request.Transaction.Amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, request.Transaction.Amount);
                            currency = Constants.Currencies.USADollar;
                        }
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = request.Game.RoundId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = request.Transaction.RefId,
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = request.Transaction.Amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        var doc = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);

                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, doc);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }

                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        response.Balance = GetBalance(client, request.Sid, request.ClientId, authToken, true);
                        response.Bonus = 0;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                }
                else
                {
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                }
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + fex.Message);
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + ex);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/Credit")]
        public ActionResult Debit([FromQuery] string authToken, [FromBody] FinOperationInput request)
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId, null, false);
                        var client = CacheManager.GetClientById(request.ClientId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var partnerAuthToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvolutionAuthToken);
                        if (partnerAuthToken != authToken)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                        if (client.Id != clientSession.Id)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                        var product = CacheManager.GetProductByExternalId(ProviderId, request.Game.Details.Table.Id);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(request.Transaction.RefId, client.Id,
                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(request.Transaction.Id.ToString(),
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentAlreadyWinned);

                        var state = (request.Transaction.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var currency = client.CurrencyId;
                        if (EvolutionHelpers.RestrictedCurrencies.Contains(client.CurrencyId))
                        {
                            request.Transaction.Amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, request.Transaction.Amount);
                            currency = Constants.Currencies.USADollar;
                        }
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = request.Game.RoundId,
                            GameProviderId = ProviderId,
                            ProductId = betDocument.ProductId,
                            TransactionId = request.Transaction.Id,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Amount = request.Transaction.Amount,
                            Client = client,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0]);
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                var message = ex.Detail == null
                                    ? new ResponseBase
                                    {
                                        ResponseCode = Constants.Errors.GeneralException,
                                        Description = ex.Message
                                    }
                                    : new ResponseBase
                                    {
                                        ResponseCode = ex.Detail.Id,
                                        Description = ex.Detail.Message
                                    };
                                Program.DbLogger.Error(JsonConvert.SerializeObject(message));
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = request.Transaction.Amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                        response.Balance = GetBalance(client, request.Sid, request.ClientId, authToken, false);
                        response.Bonus = 0;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                }
                else
                {
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                }
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + fex.Message);
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + ex);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/Cancel")]
        public ActionResult Cancel([FromQuery] string authToken, [FromBody]FinOperationInput request)//cancel bets
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId, null, false);

						var client = CacheManager.GetClientById(request.ClientId);
						if (client == null)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
						var partnerAuthToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvolutionAuthToken);
                        if (partnerAuthToken != authToken)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                        if (client.Id != clientSession.Id)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                        var product = CacheManager.GetProductByExternalId(ProviderId, request.Game.Details.Table.Id);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(request.Transaction.RefId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                        if (betDocument.State != (int)BetDocumentStates.Deleted)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                GameProviderId = ProviderId,
                                TransactionId = request.Transaction.RefId,
                                ProductId = product.Id
                            };
                            var doc = documentBl.RollbackProductTransactions(operationsFromProduct);
                            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client, operationsFromProduct, doc[0]);
                                }
                                catch (Exception ex)
                                {
                                    Program.DbLogger.Error(ex.Message);
                                }
                            }
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }
                        var currency = client.CurrencyId;
                        if (EvolutionHelpers.RestrictedCurrencies.Contains(client.CurrencyId))
                            currency = Constants.Currencies.USADollar;
                        response.Balance = GetBalance(client, request.Sid, request.ClientId, authToken, false);
                        response.Bonus = 0;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                }
                else
                {
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                }
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + fex.Message);
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(request) + "Error: response " + JsonConvert.SerializeObject(response) + "Description: " + ex);
            }
            return Ok(response);
        }

        private static decimal GetBalance(BllClient client, string sid, int clientId, string authToken, bool checkTokenExpiration )
        {
            decimal balance = 0;
            var currency = client.CurrencyId;
            if (EvolutionHelpers.RestrictedCurrencies.Contains(client.CurrencyId))
                currency = Constants.Currencies.USADollar;

            var clientSession = ClientBll.GetClientProductSession(sid, Constants.DefaultLanguageId, null, checkTokenExpiration);

            var partnerAuthToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EvolutionAuthToken);
            if (partnerAuthToken != authToken)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

            if (clientSession.Id != clientId)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongParameters);


            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
            if (isExternalPlatformClient)
            {
                ClientBll.GetClientPlatformSession(client.Id, clientSession.ParentId ?? 0);
                var balanceOutput = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, balanceOutput);
            }
            else
            {
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance);
            }
            return Math.Round(balance, 2);
        }
    }
}
