
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.TwoWinPower;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using System.Web;
using IqSoft.CP.Common.Helpers;
using System.Linq;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class TwoWinPowerController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.TwoWinPower);
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Constants.Currencies.TurkishLira
        };

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TwoWinPower).Id;
        [HttpPost]
        [Route("{partnerId}/api/TwoWinPower/ApiRequest")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                int clientId = 0;
                Int32.TryParse(input.player_id, out clientId);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                CheckSign(input, client.PartnerId);
                if (!string.IsNullOrEmpty(input.amount) && UnsuppordedCurrenies.Contains(client.CurrencyId))
                    input.amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, Convert.ToDecimal(input.amount)).ToString();

                switch (input.action)
                {
                    case TwoWinPowerHelpers.Action.Balance:
                        response.Balance = GetBalance(client.Id, 0);
                        break;
                    case TwoWinPowerHelpers.Action.Bet:
                        response = DoBet(input, client.PartnerId, client);
                        break;
                    case TwoWinPowerHelpers.Action.Win:
                        response = DoWin(input, client.PartnerId, client);
                        break;
                    case TwoWinPowerHelpers.Action.Refund:
                        response = Refund(input, client.PartnerId, client);
                        break;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.ErrorCode = TwoWinPowerHelpers.GetError(fex.Detail.Id);
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ErrorCode = TwoWinPowerHelpers.ErrorCodes.InternalError;
                    response.ErrorDescription = fex.Message;
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + ", Input:" + JsonConvert.SerializeObject(input));
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.ErrorCode = TwoWinPowerHelpers.ErrorCodes.InternalError;
                response.ErrorDescription = ex.Message;
            }
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(
                response,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }),
                Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            return httpResponse;
        }

        private BaseOutput DoBet(BaseInput input, int partnerId, BllClient client)
        {
            var clientSession = ClientBll.GetClientProductSession(input.session_id, Constants.DefaultLanguageId);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            long transId = 0;
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                    clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var document = documentBl.GetDocumentByExternalId(input.transaction_id, client.Id,
                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (document == null)
                    {
                        var amount = Convert.ToDecimal(input.amount);
                        if (input.type.ToLower() == TwoWinPowerHelpers.ActionTypes.Freespin)
                        {
                            amount = 0;
                        }
                        else
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ExternalProductId = input.game_uuid,
                                ProductId = clientSession.ProductId,
                                TransactionId = input.transaction_id,
                                OperationItems = new List<OperationItemFromProduct>()
                            };

                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            transId = document.Id;
                            document.State = (int)BetDocumentStates.Lost;

                            var recOperationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.game_uuid,
                                ProductId = clientSession.ProductId,
                                TransactionId = input.transaction_id + "_win",
                                CreditTransactionId = document.Id,
                                State = (int)BetDocumentStates.Lost,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            recOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
							clientBl.CreateDebitsToClients(recOperationsFromProduct, document, documentBl);

                            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    var balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                        clientSession.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
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
                                BaseHelpers.BroadcastBalance(client.Id);
                            }
                        }
                    }
                    else
                    {
                        transId = document.Id;
                    }
                    return new BaseOutput
                    {
                        Balance = GetBalance(client.Id, clientSession.ProductId),
                        TransactionId = transId.ToString()
                    };
                }
            }
        }

        private BaseOutput DoWin(BaseInput input, int partnerId, BllClient client)
        {
            var clientSession = ClientBll.GetClientProductSession(input.session_id, Constants.DefaultLanguageId, null, false);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                        clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var winDocument = documentBl.GetDocumentByExternalId(input.transaction_id,
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                    var resp = new BaseOutput();

                    if (winDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
                            TransactionId = input.transaction_id + "_bet",
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
                        var amount = Convert.ToDecimal(input.amount);
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;

                        operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = input.game_uuid,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.transaction_id,
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
                        resp.TransactionId = doc[0].Id.ToString();

                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                resp.Balance = ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    betDocument.Id, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                            }
                        }
                        else
                        {
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            resp.Balance = GetBalance(client.Id, clientSession.ProductId);
                        }
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            BetAmount = betDocument?.Amount,
                            Amount = amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = partnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    else
                    {
                        resp.TransactionId = winDocument.Id.ToString();
                        resp.Balance = GetBalance(client.Id, clientSession.ProductId);
                    }
                    return resp;
                }
            }
        }

        private BaseOutput Refund(BaseInput input, int partnerId, BllClient client)
        {
            var clientSession = ClientBll.GetClientProductSession(input.session_id, Constants.DefaultLanguageId, null, false);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            long transId = 0;
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
				clientSession.ProductId);
				if (partnerProductSetting == null || partnerProductSetting.Id == 0)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
				var operationsFromProduct = new ListOfOperationsFromApi
				{
                    SessionId = clientSession.SessionId,
                    GameProviderId = ProviderId,
					TransactionId = input.bet_transaction_id,
					ProductId = clientSession.ProductId
				};
				var betDocument = documentBl.GetDocumentByExternalId(input.bet_transaction_id, client.Id, ProviderId,
					partnerProductSetting.Id, (int)OperationTypes.Bet);
				var response = new BaseOutput
				{
					TransactionId = input.transaction_id
				};
				if (betDocument != null && betDocument.State != (int)BetDocumentStates.Deleted)
				{
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct)[0];
                    transId = doc.Id;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client, 
                                operationsFromProduct, doc, WebApiApplication.DbLogger);
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex.Message);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    response.TransactionId = transId.ToString();
				}
                response.Balance = GetBalance(client.Id, clientSession.ProductId);
                return response;
			}
        }

        private void CheckSign(object input, int partnerId)
        {
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TwoWinPowerId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TwoWinPowerSecretKey);
            var sign = HttpContext.Current.Request.Headers.Get("X-Sign");
            var timestamp = HttpContext.Current.Request.Headers.Get("X-Timestamp");
            var nonce = HttpContext.Current.Request.Headers.Get("X-Nonce");
          
            SortedDictionary<string, string> sortedParams = new SortedDictionary<string, string>
            {
                { "X-Merchant-Id", merchantId},
                { "X-Timestamp", timestamp },
                { "X-Nonce", nonce }
            };
            var header = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + par.Value + "&");
            sortedParams = new SortedDictionary<string, string>();
            var properties = input.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(input, null);
                if (value != null)
                    sortedParams.Add(field.Name, value.ToString());
            }
            var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + par.Value + "&");
            result = result.Remove(result.LastIndexOf("&"), 1);
            var signature = CommonFunctions.ComputeHMACSha1(header + result, secretKey).ToLower();
            if (sign.ToLower() != signature.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        private decimal GetBalance(int clientId, int productId)
        {
            var client = CacheManager.GetClientById(clientId);
            decimal balance;

            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
            if (isExternalPlatformClient)
                balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
            else
                balance = BaseHelpers.GetClientProductBalance(client.Id, productId);
            if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                return Math.Round(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance), 2);
            return Math.Round(balance, 2);
        }
	}
}
