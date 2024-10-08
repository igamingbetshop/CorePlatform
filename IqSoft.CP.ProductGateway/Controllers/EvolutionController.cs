using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Http;
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
using System.Net;
using System.Text;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;
using System.IO;
using System.Web;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class EvolutionController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Evolution);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Evolution).Id;
        [HttpPost]
        [Route("{partnerId}/api/Evolution/Check")]
        public HttpResponseMessage CheckSession([FromUri] string authToken, [FromBody] CheckInput request)
        {
            var response = new CheckOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    BaseBll.CheckIp(WhitelistedIps);
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()} " +
                                                 $"Response: {JsonConvert.SerializeObject(response)}");

            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject( response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/Balance")]
        public HttpResponseMessage Balance([FromUri]string authToken, [FromBody] BalanceInput request)
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response.Balance = GetBalance(client, request.Sid, request.ClientId, authToken, true);
                response.Bonus = 0;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                else
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()} " +
                                                 $"Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/debit")]
        public HttpResponseMessage DoBet([FromUri]string authToken, [FromBody] FinOperationInput request)// place bet
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                        var product = CacheManager.GetProductByExternalId(ProviderId, request.Game.Details.Table.Id) ??
                           throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0 || partnerProductSetting.State != (int)PartnerProductSettingStates.Active)
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
                        var doc = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);

                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                response.Balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue),
                                    client, clientSession.ParentId ?? 0, operationsFromProduct, doc, WebApiApplication.DbLogger);
                                BaseHelpers.BroadcastBalance(client.Id, response.Balance);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        else
                        {
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            response.Balance = GetBalance(client, request.Sid, request.ClientId, authToken, true);
                        }
                        
                        response.Bonus = 0;
                    }
				}
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                else
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()} " +
                                                 $"Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/Credit")]
        public HttpResponseMessage DoWin([FromUri]string authToken, [FromBody] FinOperationInput request)
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                        var product = CacheManager.GetProductByExternalId(ProviderId, request.Game.Details.Table.Id) ??
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0 || partnerProductSetting.State != (int)PartnerProductSettingStates.Active)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(request.Transaction.RefId, client.Id,
                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(request.Transaction.Id.ToString(),
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentAlreadyWinned);

                        var state = request.Transaction.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
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
                                    (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0], WebApiApplication.DbLogger);
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
                                WebApiApplication.DbLogger.Error("DebitException_" + JsonConvert.SerializeObject(message));
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            BetId = betDocument?.Id ?? 0,
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            BetAmount = betDocument?.Amount,
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
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                else
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()} " +
                                                 $"Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/Evolution/Cancel")]
        public HttpResponseMessage Cancel([FromUri]string authToken, [FromBody]FinOperationInput request)//cancel bets
        {
            var response = new StandartOutput
            {
                Status = EvolutionHelpers.Statuses.Ok,
                Uuid = request.Uuid
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                                    ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), 
                                        client, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex);
                                }
                            }
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
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
                    response.Status = EvolutionHelpers.GetResponseStatus(fex.Detail.Id);
                else
                    response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()} " +
                                                 $"Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = EvolutionHelpers.Statuses.FinalErrorActionFailed;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        private decimal GetBalance(BllClient client, string sid, int clientId, string authToken, bool checkTokenExpiration )
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
                if (checkTokenExpiration)
                    ClientBll.GetClientPlatformSession(client.Id, clientSession.ParentId ?? 0);
                var balanceOutput = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, balanceOutput);
            }
            else
            {
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId));
            }
            return Math.Round(balance, 2);
        }
    }
}
