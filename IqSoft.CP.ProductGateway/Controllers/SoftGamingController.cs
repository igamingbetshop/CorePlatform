using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Models.SoftGaming;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;
using System.Web;
using System.IO;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class SoftGamingController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftGaming).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.SoftGaming);
        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.ArgentinianPeso,
            Constants.Currencies.ColumbianPeso,
            Constants.Currencies.IranianTuman,
            Constants.Currencies.USDT
        };

        private static string GenerateHmac(String secret_key, string value)
        {
            SHA256 hash = new SHA256Managed();
            Byte[] hash_secret = hash.ComputeHash(Encoding.UTF8.GetBytes(secret_key));
            HMAC hmac = new HMACSHA256(hash_secret);
            byte[] hmac_byte = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            String hmac_value = "";
            foreach (Byte b in hmac_byte)
            {
                hmac_value = String.Concat(hmac_value, b.ToString("x2"));
            }
            return hmac_value.ToLower();
        }


        [HttpPost]
        [Route("{partnerId}/api/SoftGaming")]
        public HttpResponseMessage ApiRequest(int partnerId, JObject jInput)
        {
            var jsonResponse = string.Empty;
            int productId = 0;
            var input = jInput.ToObject<BaseInput>();
            var secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SoftGamingSecureKey);
            BllClient identity = null;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var hmac = input.hmac.ToLower();
                jInput["hmac"] = string.Empty;
                if (string.IsNullOrEmpty(secureKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidSecretKey);
                var sortedParams = jInput.ToObject<SortedDictionary<string, object>>();
                var hashString = GenerateHmac(secureKey, sortedParams.Aggregate(string.Empty, (current, par) => current + par.Value));
                if (hashString != hmac)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                long? sessionId = null;
                switch (input.type)
                {
                    case SoftGamingHelpers.ActionTypes.Ping:
                        break;
                    case SoftGamingHelpers.ActionTypes.GetBalance:
                        identity = CheckSession(input, out productId);
                        break;
                    case SoftGamingHelpers.ActionTypes.DoBet:
                        if (string.IsNullOrEmpty(input.i_rollback))
                            identity = DoBet(input, out productId);
                        else
                            identity = Rollback(input, out productId);
                        break;
                    case SoftGamingHelpers.ActionTypes.DoWin:
                        if (string.IsNullOrEmpty(input.i_rollback))
                        {
                            identity = DoWin(input, out productId, out SessionIdentity clientSession);
                            sessionId = clientSession?.SessionId;
                        }
                        else
                            identity = Rollback(input, out productId);
                        break;
                }

                string balance = "0.00";
                if (identity != null)
                {
                    var bal = Math.Round(BaseHelpers.GetClientProductBalance(identity.Id, productId), 2);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(identity, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        bal = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), identity.Id);
                    }
                    if (NotSupportedCurrencies.Contains(identity.CurrencyId))
                        bal = BaseBll.ConvertCurrency(identity.CurrencyId, Constants.Currencies.USADollar, bal);

                    balance = bal.ToString("0.00");
                }

                jsonResponse = identity == null ?
                    JsonConvert.SerializeObject(new
                    {
                        status = "OK",
                        hmac = GenerateHmac(secureKey, "OK")
                    }, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }) :
                    JsonConvert.SerializeObject(new
                    {
                        status = "OK",
                        balance = balance,
                        input.tid,
                        hmac = GenerateHmac(secureKey, string.Format("{0}{1}{2}", balance, "OK", input.tid))
                    }, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var balance = identity == null ? "0.00" : Math.Round(BaseHelpers.GetClientProductBalance(identity.Id, productId), 2).ToString("0.00");
                jsonResponse = fex.Detail.Id == Constants.Errors.ClientNotFound ? JsonConvert.SerializeObject(new
                {
                    error = fex.Detail.Message,
                    hmac = GenerateHmac(secureKey, fex.Detail.Message)
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }) : JsonConvert.SerializeObject(new
                {
                    error = fex.Detail.Message,
                    balance = balance,
                    hmac = GenerateHmac(secureKey, string.Format("{0}{1}", balance, fex.Detail.Message))
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                if(fex.Detail.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var balance = identity == null ? "0.00" : Math.Round(BaseHelpers.GetClientProductBalance(identity.Id, productId), 2).ToString("0.00");
                var response = new
                {
                    error = ex.Message,
                    balance = balance,
                    hmac = GenerateHmac(secureKey, string.Format("{0}{1}", balance, ex.Message))
                };
                jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }

        private BllClient CheckSession(BaseInput input, out int productId)
        {
            var clientSession = ClientBll.GetClientProductSession(input.i_extparam, Constants.DefaultLanguageId);
            productId = clientSession.ProductId;

            var client = CacheManager.GetClientById(clientSession.Id);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

            //
            if (NotSupportedCurrencies.Contains(client.CurrencyId))
            {
                if (input.currency.ToLower() != Constants.Currencies.USADollar.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                input.currency = client.CurrencyId;
                var amount = Convert.ToDecimal(input.amount);
                input.amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount).ToString();
            }
            //

            if (clientSession.CurrencyId.ToLower() != input.currency.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

            return client;
        }

        private BllClient DoBet(BaseInput input, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.i_extparam, Constants.DefaultLanguageId);
                    productId = clientSession.ProductId;

                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    //
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    {
                        if (input.currency.ToLower() != Constants.Currencies.USADollar.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        input.currency = client.CurrencyId;
                        var amount = Convert.ToDecimal(input.amount);
                        input.amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount).ToString();
                    }
                    //

                    if ("us" + client.Id.ToString() != input.userid || client.CurrencyId.ToLower() != input.currency.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(input.tid.ToString(), client.Id,
                          ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (betDocument != null)
                    {
                        if (betDocument.ClientId != client.Id || betDocument.Amount != Convert.ToDecimal(input.amount))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                        return client;
                    }

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        RoundId = input.i_gameid,
                        TransactionId = input.tid,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = Convert.ToDecimal(input.amount),
                        DeviceTypeId = clientSession.DeviceType
                    });
                    betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            var balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                clientSession.ParentId ?? 0, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                            BaseHelpers.BroadcastBalance(client.Id, balance);
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
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    return client;
                }
            }
        }

        private BllClient DoWin(BaseInput input, out int productId, out SessionIdentity clientSession)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    clientSession = null;
                    if (!string.IsNullOrEmpty(input.i_extparam))
                        clientSession = ClientBll.GetClientProductSession(input.i_extparam, Constants.DefaultLanguageId, null, false);
                    var product = clientSession != null ? CacheManager.GetProductById(clientSession.ProductId) :
                                                          CacheManager.GetProductByExternalId(ProviderId, input.i_gamedesc);
                    productId = product == null ? 0 : product.Id;
                    var client = CacheManager.GetClientById(clientSession != null ? clientSession.Id :
                        Convert.ToInt32(input.userid.Replace("us", string.Empty).Replace("u", string.Empty)));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    //
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    {
                        if (input.currency.ToLower() != Constants.Currencies.USADollar.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        input.currency = client.CurrencyId;
                        var amount = Convert.ToDecimal(input.amount);
                        input.amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount).ToString();
                    }
                    if ((("us" + client.Id.ToString() != input.userid) && ("u" + client.Id.ToString() != input.userid)) || client.CurrencyId.ToLower() != input.currency.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    BllPartnerProductSetting partnerProductSetting = new BllPartnerProductSetting();
                    if (clientSession == null)
                    {
                        var existingBetTransaction1 = documentBl.GetDocumentOnlyByExternalId(input.tid, ProviderId, client.Id, (int)OperationTypes.Bet);
                        if (existingBetTransaction1 != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);
                    }
                    else
                    {
                        if (product.GameProviderId != ProviderId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var existingBetTransaction = documentBl.GetDocumentByExternalId(input.tid, client.Id, ProviderId,
                            partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (existingBetTransaction != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);
                    }
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.i_gameid, ProviderId, client.Id);

                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession?.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            RoundId = input.i_gameid,
                            TransactionId = string.Format("FREEROUNDS_{0}", input.tid),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0,
                            DeviceTypeId = clientSession?.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    else if (betDocument.ProductId != null && product == null)
                    {
                        productId = betDocument.ProductId.Value;
                        product = CacheManager.GetProductById(productId);
                    }
                    Document winDocument;
                    if (clientSession != null)
                    {
                        winDocument = documentBl.GetDocumentByExternalId(input.tid, client.Id, ProviderId,
                        partnerProductSetting.Id, (int)OperationTypes.Win);
                    }
                    else
                        winDocument = documentBl.GetDocumentOnlyByExternalId(input.tid, ProviderId, client.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        var amount = Convert.ToDecimal(input.amount);
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession?.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.i_gameid,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.tid,
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

                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
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
                                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(message));
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
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
                            Amount = amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    else
                    {
                        if (winDocument.ClientId != client.Id || winDocument.Amount != Convert.ToDecimal(input.amount))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    }
                    return client;
                }
            }
        }

        private BllClient Rollback(BaseInput input, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.i_extparam, Constants.DefaultLanguageId, checkExpiration: false);
                    productId = clientSession.ProductId;

                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    //
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    {
                        if (input.currency.ToLower() != Constants.Currencies.USADollar.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        input.currency = client.CurrencyId;
                        var amount = Convert.ToDecimal(input.amount);
                        input.amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount).ToString();
                    }
                    //

                    if ("us" + client.Id.ToString() != input.userid || client.CurrencyId.ToLower() != input.currency.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.i_rollback,
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId
                    };
                    var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                    if (documents == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client, 
                                operationsFromProduct, documents[0], WebApiApplication.DbLogger);
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    return client;
                }
            }
        }
    }
}