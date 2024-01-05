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
        public HttpResponseMessage CheckSession(int partnerId, [FromUri]SessionInput input)
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

                    var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (partnerId != client.PartnerId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    if (product.GameProviderId != ProviderId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                    var sign = CommonFunctions.ComputeSha1(input.token + salt);
                    if (sign.ToLower() != input.sign.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var responseObject = new SessionOutput
                    {
                        Player = client.Id.ToString(),
                        Currency = client.CurrencyId,
                        Game = product.ExternalId
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/bet")]
        public HttpResponseMessage DoBet(int partnerId, BetInput input)
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
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);

                        var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                        var client = CacheManager.GetClientById(clientSession.Id);
                        if (partnerId != client.PartnerId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        if (product.GameProviderId != ProviderId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                        sign = CommonFunctions.ComputeSha1(sign + salt);
                        if (sign.ToLower() != input.sign.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                        var document = documentBl.GetDocumentByExternalId(input.id.ToString(), client.Id,
                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (document != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
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
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                        var balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * Rate;

                        var responseObject = new BetOutput
                        {
                            TransactionId = document.Id.ToString(),
                            Balance = Convert.ToInt64(balance)
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/win")]
        public HttpResponseMessage DoWin(int partnerId, WinInput input)
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
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);
                        var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, false);
                        var client = CacheManager.GetClientById(clientSession.Id);
                        if (partnerId != client.PartnerId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        if (product.GameProviderId != ProviderId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                        sign = CommonFunctions.ComputeSha1(sign + salt);
                        if (sign.ToLower() != input.sign.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        if (input.betTransactionId == 0)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var betDocument = documentBl.GetDocumentByExternalId(input.betTransactionId.ToString(), client.Id,
                                                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        if (input.amount <= 0)
                            input.id = input.betTransactionId;

                        var winDocument = documentBl.GetDocumentByExternalId(input.id.ToString(), client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var amount = Convert.ToDecimal(input.amount) / Rate;
                            var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.gameId.ToString(),
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
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
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            var balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * Rate;
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = Convert.ToDecimal(input.amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = partnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                            var responseObject = new BetOutput
                            {
                                TransactionId = winDocument.Id.ToString(),
                                Balance = Convert.ToInt64(balance)
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpGet]
        [Route("{partnerId}/api/endorphina/balance")]
        public HttpResponseMessage GetBalance(int partnerId, [FromUri]SessionInput input)
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

                    var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (partnerId != client.PartnerId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    if (product.GameProviderId != ProviderId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                    var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                    sign = CommonFunctions.ComputeSha1(sign + salt);
                    if (sign.ToLower() != input.sign.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * Rate;
                    var responseObject = new BetOutput
                    {
                        Balance = Convert.ToInt64(balance)
                    };

                    jsonResponse = JsonConvert.SerializeObject(responseObject, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/refund")]
        public HttpResponseMessage Refund(int partnerId, BetInput input)
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
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EndorphinaSalt);

                        var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                        var client = CacheManager.GetClientById(clientSession.Id);
                        if (partnerId != client.PartnerId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        if (product.GameProviderId != ProviderId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var sign = Uri.UnescapeDataString(CommonFunctions.GetSortedValuesAsString(input, string.Empty));
                        sign = CommonFunctions.ComputeSha1(sign + salt);
                        if (sign.ToLower() != input.sign.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            GameProviderId = ProviderId,
                            TransactionId = input.id.ToString(),
                            ExternalProductId = product.ExternalId,
                            ProductId = clientSession.ProductId
                        };
                        var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        if (documents == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        var documentId = documents[0].Id;
                        var balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * Rate;
                        var responseObject = new BetOutput
                        {
                            TransactionId = documentId.ToString(),
                            Balance = Convert.ToInt64(balance)
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpGet]
        [Route("{partnerId}/api/endorphina/check")]
        public HttpResponseMessage CheckMethod(int partnerId, [FromUri]CheckInput input)
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }
    }
}
