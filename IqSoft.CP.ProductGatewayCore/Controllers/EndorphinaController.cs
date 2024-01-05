using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Endorphina;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class EndorphinaController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "178.33.61.104",
            "88.198.136.152",
            "5.79.127.22",
            "159.69.160.125"
        };

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Endorphina).Id;
        private readonly int Rate = 1000;
        [HttpGet]
        [Route("{partnerId}/api/endorphina/session")]
        public ActionResult CheckSession(int partnerId, [FromQuery] SessionInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(jsonResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/bet")]
        public ActionResult DoBet(int partnerId, BetInput input)
        {
            var jsonResponse = string.Empty;
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
                        document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client,
                              client.Id, client.CurrencyId).AvailableBalance * Rate;

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
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(jsonResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/win")]
        public ActionResult DoWin(int partnerId, WinInput input)
        {
            var jsonResponse = string.Empty;
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
                            var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client,
                                  client.Id, client.CurrencyId).AvailableBalance * Rate;
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
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(jsonResponse);
        }

        [HttpGet]
        [Route("{partnerId}/api/endorphina/balance")]
        public ActionResult GetBalance(int partnerId, [FromQuery] SessionInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                    var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client,
                                client.Id, client.CurrencyId).AvailableBalance * Rate;
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
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(jsonResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/endorphina/refund")]
        public ActionResult Refund(int partnerId, BetInput input)
        {
            var jsonResponse = string.Empty;
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
                        BaseHelpers.BroadcastBalance(client.Id);
                        var documentId = documents[0].Id;
                        var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client,
                        client.Id, client.CurrencyId).AvailableBalance * Rate;
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
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(jsonResponse);
        }

        [HttpGet]
        [Route("{partnerId}/api/endorphina/check")]
        public ActionResult CheckMethod(int partnerId, [FromQuery] CheckInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(jsonResponse);
            }
            catch (Exception ex)
            {
                var response = new ErrorOutput
                {
                    Code = EndorphinaHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Message = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                Response.StatusCode = (int)EndorphinaHelpers.GetStatusCode(response.Code);
                Program.DbLogger.Error(ex);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(jsonResponse);
        }
    }
}