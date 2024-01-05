using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SkyWind;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class SkyWindController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.SkyWind);

		private static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SkyWind).Id;

        [HttpPost]
        [Route("{partnerId}/api/SkyWind/api/validate_ticket")]
        public HttpResponseMessage ValidateTicket(int partnerId, BaseInput input)
        {
			var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var response = new ValidateTicketOutput { ErrorCode = 0, IsTest = "false"/*to be changed*/ };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                BllClient client;
                var clientSession = CheckAuthorizationDetails(input, partnerId, out client, true, true);
                response.CustomerSessionId = input.ticket;
                response.CustomerId = client.Id.ToString();
                response.CurrencyCode = client.CurrencyId;
                response.Language = clientSession.LanguageId;
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
				var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                response.ErrorCode = SkyWindHelpers.GetErrorCode(faultException.Detail.Id);
                response.ErrorMsg = faultException.Detail.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
			}
            catch (Exception ex)
            {
				WebApiApplication.DbLogger.Error(ex);
				response.ErrorCode = SkyWindHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.ErrorMsg = ex.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
			httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyWind/api/get_balance")]
        public HttpResponseMessage GetBalance(int partnerId, BaseInput input)
        {
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var response = new GetBalanceOutput { ErrorCode = 0 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                BllClient client;
                CheckAuthorizationDetails(input, partnerId, out client, false);
                response.currency_code = client.CurrencyId;
                response.balance = BaseHelpers.GetClientProductBalance(client.Id, 0);
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                response.ErrorCode = SkyWindHelpers.GetErrorCode(faultException.Detail.Id);
                response.ErrorMsg = faultException.Detail.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.ErrorCode = SkyWindHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.ErrorMsg = ex.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyWind/api/debit")]
        public HttpResponseMessage Credit(int partnerId, DebitInput input)
        {
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var response = new DebitOutput { ErrorCode = 0 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                BllClient client;
                var clientSession = CheckAuthorizationDetails(input, partnerId, out client, false);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                        clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var document = documentBl.GetDocumentByExternalId(input.trx_id, client.Id,
                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (document != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            RoundId = input.event_id.ToString(),
                            GameProviderId = ProviderId,
                            ExternalOperationId = null,
                            ExternalProductId = input.game_id,
                            ProductId = clientSession.ProductId,
                            TransactionId = input.trx_id,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.amount,
                            DeviceTypeId = (int)SkyWindHelpers.MapDeviceType(input.platform)
                        });
						clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                        response.Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                        response.BetId = input.trx_id;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                response.ErrorCode = SkyWindHelpers.GetErrorCode(faultException.Detail.Id);
                response.ErrorMsg = faultException.Detail.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.ErrorCode = SkyWindHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.ErrorMsg = ex.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
			
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyWind/api/credit")]
        public HttpResponseMessage Debit(int partnerId, CreditInput input)
        {
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var response = new DebitOutput { ErrorCode = 0 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                BllClient client;
                var clientSession = CheckAuthorizationDetails(input, partnerId, out client, false);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                        clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var betDocument = documentBl.GetDocumentByExternalId(input.trx_id, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                        var winDocument = documentBl.GetDocumentByExternalId(input.trx_id, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.TransactionAlreadyExists);

                        var state = (input.amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            RoundId = input.event_id.ToString(),
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = input.game_id,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.trx_id,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            Info = input.event_type,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.amount
                        });
						clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        response.Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                        response.BetId = input.trx_id;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                response.ErrorCode = SkyWindHelpers.GetErrorCode(faultException.Detail.Id);
                response.ErrorMsg = faultException.Detail.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                response.ErrorCode = SkyWindHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.ErrorMsg = ex.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyWind/api/rollback")]
        public HttpResponseMessage RollbackBet(int partnerId, DebitInput input)
        {
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var response = new DebitOutput { ErrorCode = 0 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                BllClient client;
                var clientSession = CheckAuthorizationDetails(input, partnerId, out client, false);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                    clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        TransactionId = input.trx_id,
                        ExternalProductId = input.game_id,
                        ProductId = clientSession.ProductId
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    response.Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                    response.BetId = input.trx_id;
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                response.ErrorCode = SkyWindHelpers.GetErrorCode(faultException.Detail.Id);
                response.ErrorMsg = faultException.Detail.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                response.ErrorCode = SkyWindHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.ErrorMsg = ex.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyWind/api/get_player")]
        public HttpResponseMessage GetPlayer(int partnerId, BaseInput input)
        {
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var response = new PlayerOutput { ErrorCode = 0 };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                BllClient client;
                var clientSession = CheckAuthorizationDetails(input, partnerId, out client, false);
                response.CustomerId = client.Id.ToString();
                response.GameGroup = string.Empty;
                response.CustomerLogin = client.Id.ToString();
                response.CurrencyCode = clientSession.CurrencyId;
                response.Language = clientSession.LanguageId;
                response.Country = clientSession.Country;
                response.FirstName = client.FirstName;
                response.LastName = client.LastName;
                response.Email = string.Empty;
                response.IsTestCusomer = "false";
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                response.ErrorMsg = faultException.Detail.Message;
                response.ErrorCode = SkyWindHelpers.GetErrorCode(faultException.Detail.Id);
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                response.ErrorCode = SkyWindHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.ErrorMsg = ex.Message;
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        private SessionIdentity CheckAuthorizationDetails(BaseInput input, int partnerId, out BllClient client, bool idValidateToken, bool refreshSession = false)
        {
            var providerMerchantId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SkyWindMerchantId);
            var providerMerchantPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SkyWindMerchantPwd);
            if (providerMerchantId != input.merch_id || providerMerchantPwd != input.merch_pwd)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
            var token = idValidateToken ? input.ticket : input.cust_session_id;
            var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
            client = CacheManager.GetClientById(clientSession.Id);
            if (partnerId != client.PartnerId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            if (product.GameProviderId != ProviderId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
            if (refreshSession)
            {

                //var newSession = clientBl.RefreshClientSession(input.ticket, true);
                //clientSession.Token = newSession.Token;
            }
            return clientSession;
        }
    }
}
