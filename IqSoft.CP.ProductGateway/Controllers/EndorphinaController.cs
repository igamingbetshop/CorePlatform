using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Endorphina;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class EndorphinaController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Endorphina);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Endorphina).Id;
        private readonly int Rate = 1000;
        [HttpGet]
        [Route("{partnerId}/api/endorphina/session")]
        public HttpResponseMessage CheckSession([FromUri] BaseInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = CommonFunctions.ComputeSha1(input.token + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var responseObject = new BaseOutput
                {
                    Player = client.Id.ToString(),
                    Currency = client.CurrencyId,
                    Game = product.ExternalId
                };
                jsonResponse = JsonConvert.SerializeObject(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");


            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }

        [HttpGet]
        [Route("{partnerId}/api/endorphina/balance")]
        public HttpResponseMessage GetBalance([FromUri] BaseInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                sign = CommonFunctions.ComputeSha1(sign + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                jsonResponse = JsonConvert.SerializeObject(new BetOutput { Balance = Convert.ToInt64(balance*1000) },
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/bet")]
        public HttpResponseMessage DoBet(BetInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                sign = CommonFunctions.ComputeSha1(sign + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var document = documentBl.GetDocumentByExternalId(input.id.ToString(), client.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        if (document == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                RoundId = input.gameId.ToString(),
                                TransactionId = input.id.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = Convert.ToDecimal(input.amount) / Rate,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue),
                                        client, clientSession.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex);
                                    documentBl.RollbackProductTransactions(operationsFromProduct);
                                    throw;
                                }
                            }
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                            }
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                        var responseObject = new BetOutput
                        {
                            TransactionId = document.Id.ToString(),
                            Balance = Convert.ToInt64(balance * Rate)
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/win")]
        public HttpResponseMessage DoWin(WinInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                sign = CommonFunctions.ComputeSha1(sign + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        if (input.betTransactionId == 0)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var betDocument = documentBl.GetDocumentByExternalId(input.betTransactionId.ToString(), client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Bet) ??
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        if (input.amount < 0)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                        if (input.amount == 0)
                            input.id ="Lost_"+input.betTransactionId;
                        var winDocument = documentBl.GetDocumentByExternalId(input.id.ToString(), client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);

                        if (winDocument == null)
                        {
                            var amount = Convert.ToDecimal(input.amount) / Rate;
                            var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.gameId.ToString(),
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.id.ToString(),
                                ProductId = betDocument.ProductId,
                                TransactionId = input.id.ToString(),
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
                            });

                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument, WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                                }
                            }
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    BetAmount = betDocument?.Amount,
                                    Amount = Convert.ToDecimal(amount),
                                    CurrencyId = client.CurrencyId,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                        var responseObject = new BetOutput
                        {
                            TransactionId = winDocument.Id.ToString(),
                            Balance = Convert.ToInt64(balance * Rate)
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/refund")]
        public HttpResponseMessage Refund(int partnerId, BetInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                sign = CommonFunctions.ComputeSha1(sign + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.id.ToString(),
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId
                    };
                    var docId = string.Empty;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);

                    try
                    {
                        var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        docId = documents[0].Id.ToString();
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    operationsFromProduct, documents[0], WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                            }
                        }
                        else
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);

                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        if (fex.Detail.Id == (int)Constants.Errors.DocumentAlreadyRollbacked ||
                           fex.Detail.Id == (int)Constants.Errors.DocumentRollbacked)
                            docId = documentBl.GetDocumentByExternalId(input.id.ToString(), clientSession.Id, ProviderId,
                                                                               partnerProductSetting.Id, (int)OperationTypes.BetRollback)?.Id.ToString();
                    }
                    var balance = 0m;
                    if (isExternalPlatformClient)
                        balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                    else
                        balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                    var responseObject = new BetOutput
                    {
                        TransactionId = docId,
                        Balance = Convert.ToInt64(balance * Rate)
                    };
                    jsonResponse = JsonConvert.SerializeObject(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }

        [HttpGet]
        [Route("{partnerId}/api/endorphina/check")]
        public HttpResponseMessage CheckMethod(int partnerId, [FromUri] CheckInput input)
        {
            var jsonResponse = string.Empty;
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                    var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EndorphinaMerchantId);
                    var sign = CommonFunctions.ComputeSha1(input.param + salt);
                    if (sign.ToLower() != input.sign.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var response = new CheckOutput
                    {
                        nodeId = merchantId,
                        param = input.param
                    };
                    response.sign = CommonFunctions.ComputeSha1(CommonFunctions.GetSortedValuesAsString(response, string.Empty) + salt);
                    jsonResponse = JsonConvert.SerializeObject(response);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/promoWin")]
        public HttpResponseMessage PromoWin(WinInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                sign = CommonFunctions.ComputeSha1(sign + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (input.amount < 0)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var transactionId = $"PromoWin_{input.promoId}_{input.promoName}";

                        var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);

                        if (winDocument == null)
                        {
                            var amount = Convert.ToDecimal(input.amount) / Rate;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                RoundId = transactionId,
                                TransactionId = transactionId,
                                OperationTypeId = (int)OperationTypes.Bet,
                                State = (int)BetDocumentStates.Won,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    clientSession.ParentId ?? 0, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex.Message);
                                    documentBl.RollbackProductTransactions(operationsFromProduct);
                                    throw;
                                }
                            }
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = transactionId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ProductId = betDocument.ProductId,
                                TransactionId = transactionId,
                                CreditTransactionId = betDocument.Id,
                                State = (int)BetDocumentStates.Won,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
                            });
                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client, betDocument.Id,
                                                                          operationsFromProduct, winDocument, WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                                }
                            }
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    BetAmount = betDocument?.Amount,
                                    Amount = Convert.ToDecimal(input.amount) / Rate,
                                    CurrencyId = client.CurrencyId,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                            var balance = 0m;
                            if (isExternalPlatformClient)
                                balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                            else
                                balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                            var responseObject = new BetOutput
                            {
                                TransactionId = winDocument.Id.ToString(),
                                Balance = Convert.ToInt64(balance * Rate)
                            };
                            jsonResponse = JsonConvert.SerializeObject(responseObject);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/bonus")]
        public HttpResponseMessage DoBonusWin(WinInput input)
        {
            var jsonResponse = "{}";
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.player)) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                sign = CommonFunctions.ComputeSha1(sign + salt);
                if (sign.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.game) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientBonusId = Convert.ToInt32(input.id);
                    var clientBonus = clientBl.GetClientBonusById(clientBonusId);
                    switch (input.state.ToLower())
                    {
                        case "expired":
                            try
                            {
                                clientBl.CancelClientFreespin(clientBonusId, false);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Info("Expired: " + JsonConvert.SerializeObject(input) +  "  Error: " + ex.Message);
                            }
                            break;
                        case "completed":
                            if (input.bonusWin > 0 && clientBonus != null && clientBonus.Status == (int)ClientBonusStatuses.Finished)
                            {
                                var transactionId = $"{Constants.FreeSpinPrefix}{clientBonus.Id}_{product.Id}";
                                var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                                if (winDocument == null)
                                {
                                    var operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        GameProviderId = ProviderId,
                                        ProductId = product.Id,
                                        TransactionId = $"Bet_{transactionId}",
                                        OperationTypeId = (int)OperationTypes.Bet,
                                        State = (int)BetDocumentStates.Uncalculated,
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = 0
                                    });
                                    var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                                    if (isExternalPlatformClient)
                                    {
                                        try
                                        {
                                            ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 0,
                                                                                     operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                                        }
                                        catch (Exception ex)
                                        {
                                            WebApiApplication.DbLogger.Error(ex);
                                            documentBl.RollbackProductTransactions(operationsFromProduct);
                                            throw;
                                        }
                                    }
                                    else
                                        BaseHelpers.BroadcastBetLimit(info);

                                    var state = input.bonusWin > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                                    betDocument.State = state;
                                    operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        RoundId = transactionId,
                                        GameProviderId = ProviderId,
                                        OperationTypeId = (int)OperationTypes.Win,
                                        ProductId = betDocument.ProductId,
                                        TransactionId = transactionId,
                                        CreditTransactionId = betDocument.Id,
                                        State = state,
                                        Info = string.Empty,
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = Convert.ToDecimal(input.bonusWin.Value) / Rate
                                    });
                                    winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                                    if (isExternalPlatformClient)
                                    {
                                        try
                                        {
                                            ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                            (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument, WebApiApplication.DbLogger);
                                        }
                                        catch (Exception ex)
                                        {
                                            WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                        BaseHelpers.BroadcastWin(new ApiWin
                                        {
                                            GameName = product.NickName,
                                            ClientId = client.Id,
                                            ClientName = client.FirstName,
                                            BetAmount = betDocument?.Amount,
                                            Amount = input.bonusWin.Value/ Rate,
                                            CurrencyId = client.CurrencyId,
                                            PartnerId = client.PartnerId,
                                            ProductId = product.Id,
                                            ProductName = product.NickName,
                                            ImageUrl = product.WebImageUrl
                                        });
                                    }
                                }
                            }
                            break;
                        default: break;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(fex.Detail.Id),
                    Message = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                httpResponse.StatusCode = EndorphinaHelpers.GetStatusCode(response.Code);
                WebApiApplication.DbLogger.Error(ex);
            }
            WebApiApplication.DbLogger.Info($"InputStrin: {JsonConvert.SerializeObject(input)}  Output: {jsonResponse}");
            httpResponse.Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            return httpResponse;
        }
    }
}