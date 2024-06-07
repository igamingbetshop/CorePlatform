using IqSoft.CP.BLL.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.ProductGateway.Models.InBet;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using System.Text;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class InBetController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.InBet).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.InBet);

        [HttpPost]
        [Route("{partnerId}/api/InBet/ApiRequest")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            var response = new BaseOutput();
            BllClient client = null;
            int productId = 0;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                if (input.Session == "FUN")
                {
                    response.Status = "OK";
                    response.Currency = "FUN";
                    response.Balance = 5000;
                }
                else
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Session, Constants.DefaultLanguageId);
                    if (clientSession == null)
                        throw new Exception("session_not_found");

                    client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    
                    var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.InBetSecretKey);
                    if (string.IsNullOrEmpty(secretKey))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidSecretKey);

                    var strToSign = string.Format("{0}::{1}::{2}::{3}",
                        input.Method, input.Session, input.Trx_id, secretKey);
                    for (int i = 0; i < 45; i++)
                    {
                        strToSign = CommonFunctions.ComputeMd5(strToSign);
                    }
                    if (strToSign.ToLower() != input.Sign.ToLower())
                        throw new Exception("sign_not_found");

                    using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(documentBl))
                        {
                            var product = CacheManager.GetProductById(clientSession.ProductId); //input.Tag.Game_id);
                            if (product == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                            productId = product.Id;

                            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                            DAL.Document betDocument = null;
                            if (input.Tag.Game_uuid != "-1")
                            {
                                betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.Tag.Game_uuid, ProviderId, client.Id);
                                if (input.Minus.HasValue && betDocument == null)
                                {
                                    var operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        SessionId = clientSession.SessionId,
                                        CurrencyId = client.CurrencyId,
                                        RoundId = input.Tag.Game_uuid,
                                        GameProviderId = ProviderId,
                                        TransactionId = input.Trx_id,
                                        ExternalProductId = input.Trx_id,
                                        ProductId = product.Id,
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = Convert.ToDecimal(input.Minus),
                                        DeviceTypeId = clientSession.DeviceType
                                    });
                                    betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                    BaseHelpers.BroadcastBalance(client.Id);
                                    BaseHelpers.BroadcastBetLimit(info);
                                }
                                if (input.Plus.HasValue && betDocument != null)
                                {
                                    var winDocument = documentBl.GetDocumentByExternalId(input.Trx_id, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                                    if (winDocument == null)
                                    {
                                        var state = (input.Plus.Value > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                                        betDocument.State = state;
                                        var operationsFromProduct = new ListOfOperationsFromApi
                                        {
                                            SessionId = clientSession.SessionId,
                                            CurrencyId = client.CurrencyId,
                                            RoundId = input.Tag.Game_uuid,
                                            GameProviderId = ProviderId,
                                            OperationTypeId = (int)OperationTypes.Win,
                                            TransactionId = input.Trx_id,
                                            ExternalProductId = input.Trx_id,
                                            ProductId = betDocument.ProductId,
                                            CreditTransactionId = betDocument.Id,
                                            State = state,
                                            OperationItems = new List<OperationItemFromProduct>()
                                        };
                                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                        {
                                            Client = client,
                                            Amount = Convert.ToDecimal(input.Plus.Value)
                                        });
                                        clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                        BaseHelpers.BroadcastWin(new ApiWin
                                        {
                                            GameName = product.NickName,
                                            ClientId = client.Id,
                                            ClientName = client.FirstName,
                                            BetAmount = betDocument?.Amount,
                                            Amount = Convert.ToDecimal(input.Plus.Value),
                                            CurrencyId = client.CurrencyId,
                                            PartnerId = client.PartnerId,
                                            ProductId = product.Id,
                                            ProductName = product.NickName,
                                            ImageUrl = product.WebImageUrl
                                        });
                                    }
                                    else if (winDocument.State == (int)BetDocumentStates.Deleted)
                                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.GeneralException);
                                }
                            }
                            response.Status = "OK";
                            response.Currency = client.CurrencyId;
                            response.Balance = BaseHelpers.GetClientProductBalance(client.Id, productId);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(new { fex.Detail.Id, Description = fex.Detail.Message });
                response.Status = "sign_not_found";
                if (fex.Detail.Id == Constants.Errors.LowBalance && client != null)
                {
                    response.Currency = client.CurrencyId;
                    response.Balance = BaseHelpers.GetClientProductBalance(client.Id, productId);
                }
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.Status = "sign_not_found";
            }

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore
				}), Encoding.UTF8)
            };
            return httpResponse;
        }
	}
}
