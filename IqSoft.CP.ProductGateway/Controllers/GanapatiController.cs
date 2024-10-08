using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Ganapati;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class GanapatiController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Ganapati).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Ganapati);

        [HttpPost]
        [Route("{partnerId}/api/Ganapati/{operatorId}/authenticate")]
        public HttpResponseMessage Authenticate(int partnerId, BaseObject input)
        {
            var response = new BaseObject();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var hash = "authenticate" + JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    var inputHash = HttpContext.Current.Request.Headers.Get("hash");
                    WebApiApplication.DbLogger.Info("Input:" + hash);
                    WebApiApplication.DbLogger.Info("Input hash:" + inputHash);
                    var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GanapatiSecretKey);
                    hash = CommonFunctions.ComputeHMACSha1(hash, secretKey);
                    if (hash.ToLower() != inputHash.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));

                    response.Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId)));
                    response.Currency = client.CurrencyId;
                    response.PlayerId = client.Id.ToString();
                    response.RefreshToken = clientBl.RefreshClientSession(input.Token, true).Token;
                    BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                    response.AccountData = new Account
                    {
                        CountryCode = clientSession.Country,
                        Gender = ((Gender)client.Gender).ToString().ToLower(),
                        Alias = client.UserName
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Description = ex.Message;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            WebApiApplication.DbLogger.Info("Output:" + jsonResponse);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/Ganapati/{operatorId}/fetchBalance")]
        public HttpResponseMessage GetBalance(int partnerId, BaseObject input)
        {
            var response = new BaseObject();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var hash = "fetchBalance" + JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var inputHash = HttpContext.Current.Request.Headers.Get("hash");
                var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GanapatiSecretKey);
                hash = CommonFunctions.ComputeHMACSha1(hash, secretKey);
                if (hash.ToLower() != inputHash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientProductSession(input.RefreshToken, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                response.Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId)));
                response.Currency = client.CurrencyId;

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Description = ex.Message;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            WebApiApplication.DbLogger.Info("Output:" + jsonResponse);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/Ganapati/{operatorId}/Withdraw")]
        public HttpResponseMessage DoBet(int partnerId, BetInput input)
        {
            var response = new BaseObject();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var hash = "withdraw" + JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var inputHash = HttpContext.Current.Request.Headers.Get("hash");
                var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GanapatiSecretKey);
                hash = CommonFunctions.ComputeHMACSha1(hash, secretKey);
                if (hash.ToLower() != inputHash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientProductSession(input.RefreshToken, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id,
                                                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                RoundId = input.GameRound,
                                ExternalProductId = input.GameId,
                                ProductId = product.Id,
                                TransactionId = input.TransactionId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount.Value,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        response.Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)));
                        response.Currency = client.CurrencyId;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Description = ex.Message;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            WebApiApplication.DbLogger.Info("Output:" + jsonResponse);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/Ganapati/{operatorId}/Deposit")]
        public HttpResponseMessage DoWin(int partnerId, BetInput input)
        {
            var response = new BaseObject();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var hash = "deposit" + JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var inputHash = HttpContext.Current.Request.Headers.Get("hash");
                WebApiApplication.DbLogger.Info("Input:" + hash);
                WebApiApplication.DbLogger.Info("Input hash:" + inputHash);
                var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GanapatiSecretKey);
                hash = CommonFunctions.ComputeHMACSha1(hash, secretKey);
                if (hash.ToLower() != inputHash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientProductSession(input.RefreshToken, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.GameRound, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id,
                                                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.GameRound,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.GameId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.TransactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount.Value
                            });
                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                BetId = betDocument?.Id ?? 0,
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = input.Amount.Value,
                                CurrencyId = client.CurrencyId,
                                PartnerId = partnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        response.Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)));
                        response.Currency = client.CurrencyId;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Description = ex.Message;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            WebApiApplication.DbLogger.Info("Output:" + jsonResponse);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/Ganapati/{operatorId}/rollback")]
        public HttpResponseMessage Rollback(int partnerId, BetInput input)
        {
            var response = new BaseObject();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var hash = "rollback" + JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var inputHash = HttpContext.Current.Request.Headers.Get("hash");
                WebApiApplication.DbLogger.Info("Input:" + hash);
                WebApiApplication.DbLogger.Info("Input hash:" + inputHash);
                var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GanapatiSecretKey);
                hash = CommonFunctions.ComputeHMACSha1(hash, secretKey);
                if (hash.ToLower() != inputHash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientProductSession(input.RefreshToken, Constants.DefaultLanguageId, null, false);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.PlayerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Bet,
                        TransactionId = input.TransactionId,
                        Info = input.Description
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct, false);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    response.Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)));
                    response.Currency = client.CurrencyId;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                response.ErrorCode = GanapatiHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Description = ex.Message;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            WebApiApplication.DbLogger.Info("Output:" + jsonResponse);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }
    }
}