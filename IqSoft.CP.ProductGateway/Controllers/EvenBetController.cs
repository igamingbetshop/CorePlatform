using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.EvenBet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class EvenBetController : ApiController
    {
        private static readonly  BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.EvenBet);
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.EvenBet);

        [HttpPost]
        [Route("{partnerId}/api/EvenBet")]
        public HttpResponseMessage ApiRequest(int partnerId, JObject input)
        {
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            var response = new BaseOutput();
            try
            {
                var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
                BaseBll.CheckIp(WhitelistedIps);
                var signature = HttpContext.Current.Request.Headers.Get("sign");
                var baseInput = JsonConvert.DeserializeObject<BaseInput>(JsonConvert.SerializeObject(input));
                var client = CacheManager.GetClientById(baseInput.UserId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var secureKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.IqSoftKeyToEvenBet);
                var sign = CommonFunctions.ComputeSha256(JsonConvert.SerializeObject(input) + secureKey);
                if (sign != signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                baseInput.TransactionId = partnerId + "_" + baseInput.TransactionId;
                if (baseInput.Amount.HasValue && baseInput.Amount > 0)
                    baseInput.Amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, baseInput.Amount.Value);
                int productId = 0;
                switch (baseInput.Method)
                {
                    case "GetBalance":                                            
                        break;
                    case "GetCash":
                        productId = DoBet(baseInput);
                        break;
                    case "ReturnCash":
                        productId = DoWin(baseInput);
                        break;
                    case "Rollback":
                        productId = Refund(baseInput);
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);
                }
                decimal balance = 0;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                {
                    var balanceOutput = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balanceOutput);
                }
                else
                {
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar,
                        BaseHelpers.GetClientProductBalance(client.Id, productId));
                }

                response.Balance = Convert.ToInt64(balance * 100);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = EvenBetHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                WebApiApplication.DbLogger.Error(e);
                var error = EvenBetHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(jsonResponse) + "_" + JsonConvert.SerializeObject(response));
            return resp;
        }

        private int DoBet(BaseInput input)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    //var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(input.UserId);
                    var product = CacheManager.GetProductByExternalId(Provider.Id, Provider.Name);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var document = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id,
                       Provider.Id, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = client.CurrencyId,
                        GameProviderId = Provider.Id,
                        ExternalProductId = product.ExternalId,
                        ProductId = product.Id,
                        TransactionId = input.TransactionId,
                        OperationTypeId = (int)OperationTypes.Bet,
                        State = (int)BetDocumentStates.Uncalculated,
                        OperationItems = new List<OperationItemFromProduct>()
                    };

                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = input.Amount.Value / 100
                    });
                    var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                    BaseHelpers.BroadcastBetLimit(info);
                    var lostOperationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = client.CurrencyId,
                        GameProviderId = Provider.Id,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalOperationId = null,
                        ExternalProductId = product.ExternalId,
                        ProductId = betDocument.ProductId,
                        TransactionId = input.TransactionId + "_lost",
                        CreditTransactionId = betDocument.Id,
                        State = (int)BetDocumentStates.Lost,
                        Info = string.Empty,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    
                    lostOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = 0,
                        PossibleWin = 0
                    });
                   
                    var doc = clientBl.CreateDebitsToClients(lostOperationsFromProduct, betDocument, documentBl);                    
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, null, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                            var balace = ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client, 
                                (betDocument == null ? (long?)null : betDocument.Id), lostOperationsFromProduct, doc[0], WebApiApplication.DbLogger);
                            BaseHelpers.BroadcastBalance(client.Id, balace);
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
                    }
                    return product.Id;
                }
            }
        }

        private int DoWin(BaseInput input)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    //var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(input.UserId);

                    var product = CacheManager.GetProductByExternalId(Provider.Id, Provider.Name);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId,
                    client.Id, Provider.Id, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = client.CurrencyId,
                        GameProviderId = Provider.Id,
                        ProductId = product.Id,
                        TransactionId = input.TransactionId + "_bet",
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
                    BaseHelpers.BroadcastBetLimit(info);
                    var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                    betDocument.State = state;

                    var winOperationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = client.CurrencyId,
                        GameProviderId = Provider.Id,
                        ProductId = product.Id,
                        TransactionId = input.TransactionId,
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    winOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Amount = input.Amount.Value / 100,
                        Client = client,
                        PossibleWin = input.Rake == null ? 0 : (decimal)input.Rake / 100000
                    });
                    var doc = clientBl.CreateDebitsToClients(winOperationsFromProduct, betDocument, documentBl);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, null, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                            var balance = ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                (betDocument == null ? (long?)null : betDocument.Id), winOperationsFromProduct, doc[0], WebApiApplication.DbLogger);
                            BaseHelpers.BroadcastBalance(client.Id, balance);
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
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = input.Amount.Value / 100,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }

                    return product.Id;
                }
            }
        }

        private int Refund(BaseInput input)
        {
            //var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var client = CacheManager.GetClientById(input.UserId);
                var product = CacheManager.GetProductByExternalId(Provider.Id, Provider.Name);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = Provider.Id,
                    TransactionId = input.ReferenceTransactionId,
                    ProductId = product.Id
                };
                var betDocument = documentBl.GetDocumentByExternalId(input.ReferenceTransactionId, client.Id, Provider.Id,
                    partnerProductSetting.Id, (int)OperationTypes.Bet);

                if (betDocument != null && betDocument.State != (int)BetDocumentStates.Deleted)
                {
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
                            WebApiApplication.DbLogger.Error(ex.Message);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                }

                return product.Id;
            }
        }
    }
}