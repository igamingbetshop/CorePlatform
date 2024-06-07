using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Text;
using IqSoft.CP.ProductGateway.Models.Igrosoft;
using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;
using System.Linq;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class IgrosoftController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Igrosoft);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Igrosoft).Id;

        [HttpPost]
        [Route("{partnerId}/api/Igrosoft/ApiRequest")]
        public HttpResponseMessage ApiRequest(int partnerId, BaseInput input)
        {
            var jsonResponse = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var provider = HttpContext.Current.Request.Headers.Get("X-Casino-Provider-Id");
                var transId = HttpContext.Current.Request.Headers.Get("X-Casino-Transaction-Id");
                var timestamp = HttpContext.Current.Request.Headers.Get("X-Casino-Timestamp");
                var signature = HttpContext.Current.Request.Headers.Get("X-Casino-Signature");
                var salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.IgrosoftSalt);
                if (CommonFunctions.ComputeMd5(provider + transId + timestamp + salt).ToLower() != signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                int productId = 0;
                if (client == null || client.PartnerId != partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                if (input.Withdraw.HasValue && input.Withdraw.Value != 0)
                    jsonResponse.TransactionId = DoBet(input, client, out productId);
                if (input.Deposit.HasValue)
                    jsonResponse.TransactionId = DoWin(input, client, out productId);
                //bonus ??
                //
                jsonResponse.Status = IgrosoftHelpers.Statuses.Success;
                jsonResponse.SessionId = input.SessionId;
                jsonResponse.Amount = (BaseHelpers.GetClientProductBalance(Convert.ToInt32(input.ClientId), productId) * 
                    (input.Denom.HasValue ? input.Denom.Value : 1)).ToString();
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                jsonResponse.Status = IgrosoftHelpers.Statuses.Error;
                jsonResponse.Message = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(jsonResponse));
            }
            catch (Exception ex)
            {
                jsonResponse.Status = IgrosoftHelpers.Statuses.Error;
                jsonResponse.Message = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(jsonResponse, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        private string DoBet(BaseInput input, BllClient client, out int productId)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            productId = clientSession.ProductId;
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
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
                            RoundId = input.GameRound,
                            GameProviderId = ProviderId,
                            ExternalProductId = input.GameId,
                            ProductId = clientSession.ProductId,
                            TransactionId = input.TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Withdraw.Value / (input.Denom.HasValue ? input.Denom.Value : 1),
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        var lostOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.GameRound,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = input.GameId,
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
                            Amount = 0
                        });

                        betDocument = clientBl.CreateDebitsToClients(lostOperationsFromProduct, betDocument, documentBl)[0];
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    }
                    return betDocument.Id.ToString();
                }
            }
        }

        private string DoWin(BaseInput input, BllClient client, out int productId)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            productId = clientSession.ProductId;
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.GameRound, ProviderId, client.Id);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        var state = (input.Deposit.Value > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
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
                            Amount = input.Deposit.Value / (input.Denom.HasValue ? input.Denom.Value : 1)
                        });

                        winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    }
                    return winDocument.Id.ToString();
                }
            }
        }
    }
}