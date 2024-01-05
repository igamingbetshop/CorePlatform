using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.EvenBet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class EvenBetController : ControllerBase
    {
        private static readonly  BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.EvenBet);
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "94.130.118.137",
            "88.198.130.123",
            "65.108.225.239",
            "2a01:4f9:1a:a5eb::14"
        };
        [HttpPost]
        [Route("{partnerId}/api/EvenBet")]
        public ActionResult ApiRequest(int partnerId, JObject input)
        {
            Program.DbLogger.Info(JsonConvert.SerializeObject(input));
            var response = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                if (!Request.Headers.ContainsKey("sign"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers["sign"];
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
                switch (baseInput.Method)
                {
                    case "GetBalance":                                            
                        break;
                    case "GetCash":
                        DoBet(baseInput);
                        break;
                    case "ReturnCash":
                        DoWin(baseInput);
                        break;
                    case "Rollback":
                        Refund(baseInput);
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
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance);
                }

                response.Balance = Convert.ToInt64(balance * 100);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = EvenBetHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                Program.DbLogger.Error(e);
                var error = EvenBetHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
            }
            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            Program.DbLogger.Info(JsonConvert.SerializeObject(jsonResponse) + "_" + JsonConvert.SerializeObject(response));
            return Ok(jsonResponse);
        }

        private static void DoBet(BaseInput input)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                    var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);

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
                            ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, null, operationsFromProduct, betDocument);
                            ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client, (betDocument == null ? (long?)null : betDocument.Id), lostOperationsFromProduct, doc[0]);
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
                    Program.DbLogger.Info("End");
                }
            }
        }

        private static void DoWin(BaseInput input)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                    var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
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
                            ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, null, operationsFromProduct, betDocument);
                            ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client, (betDocument == null ? (long?)null : betDocument.Id), winOperationsFromProduct, doc[0]);
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
                        Amount = input.Amount.Value / 100,
                        CurrencyId = client.CurrencyId,
                        PartnerId = client.PartnerId,
                        ProductId = product.Id,
                        ProductName = product.NickName,
                        ImageUrl = product.WebImageUrl
                    });
                }
            }
        }

        private static void Refund(BaseInput input)
        {
            //var clientSession = ClientBll.GetClientProductSession(request.Sid, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
            }
        }
    }
}