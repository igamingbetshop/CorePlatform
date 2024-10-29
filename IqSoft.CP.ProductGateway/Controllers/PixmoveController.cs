using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.ProductGateway.Helpers;
using System.Net.Http.Headers;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Models.Pixmove;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class PixmoveController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Pixmove);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Pixmove).Id;

        [HttpGet]
        [Route("{partnerId}/api/pixmove/balance")]
        public HttpResponseMessage GetBalance([FromUri]BaseInput input)
        {
            var baseOutput = new BaseOutput ();           
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var operatorIdKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmovePartnerId);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmoveApiKey);
                if (operatorIdKey != input.partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
                var hash = CommonFunctions.ComputeHMACSha256($"balance{client.CurrencyId}{operatorIdKey}{client.Id}" +
                                                             $"{clientSession.Token}{apiKey}", apiKey).ToLower();
                if(hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                if (client.Id.ToString() != input.playerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != input.currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                baseOutput.Data = new
                {
                    balance = Math.Round(balance, 2)
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id)
                };
                
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");

            }
            catch (Exception ex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg( Constants.Errors.GeneralException)
                };
                WebApiApplication.DbLogger.Error($"Code: {Constants.Errors.GeneralException} Message: {ex} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/pixmove/bet")]
        public HttpResponseMessage DoBet(TransactionInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);               
                var clientSession = ClientBll.GetClientProductSession(input.sessionId, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var operatorIdKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmovePartnerId);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmoveApiKey);
                if (operatorIdKey != input.partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
                var hash = CommonFunctions.ComputeHMACSha256($"bet{input.amount}{client.CurrencyId}{operatorIdKey}{client.Id}" +
                                                             $"{input.roundId}{clientSession.Token}{input.transactionId}{apiKey}", apiKey).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId )
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                if (client.Id.ToString() != input.playerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != input.currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                long transId = 0;
                var transactionInput = new Models.BaseModels.TransactionInput
                {
                    Client = client,
                    ProviderId = ProviderId,
                    ProductExternalId = product.ExternalId,
                    SessionId = clientSession.SessionId,
                    SessionParentId = clientSession.ParentId ?? 0,
                    SessionDeviceType = clientSession.DeviceType,
                    Amount = Convert.ToDecimal(input.amount),
                    TransactionId = input.transactionId,
                    RoundId = input.roundId
                };                               
                transId = BaseHelpers.DoBet(transactionInput).Id;
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                baseOutput.Data = new
                {
                    balance = Math.Round(balance, 2),
                    transactionId = transId
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id)
                };
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            catch (Exception ex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException)
                };
                WebApiApplication.DbLogger.Error($"Code: {Constants.Errors.GeneralException} Message: {ex} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/pixmove/win")]
        public HttpResponseMessage DoWin(TransactionInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.sessionId, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var operatorIdKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmovePartnerId);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmoveApiKey);
                if (operatorIdKey != input.partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
                var hash = CommonFunctions.ComputeHMACSha256($"win{input.amount}{client.CurrencyId}{operatorIdKey}{client.Id}" +
                                                             $"{input.roundId}{clientSession.Token}{input.transactionId}{apiKey}", apiKey).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                if (client.Id.ToString() != input.playerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != input.currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                long transId = 0;
                var transactionInput = new Models.BaseModels.TransactionInput
                {
                    Client = client,
                    ProviderId = ProviderId,
                    ProductExternalId = product.ExternalId,
                    SessionId = clientSession.SessionId,
                    SessionParentId = clientSession.ParentId ?? 0,
                    SessionDeviceType = clientSession.DeviceType,
                    Amount =Convert.ToDecimal(input.amount),
                    TransactionId = input.transactionId,
                    RoundId = input.roundId
                };
                transId = BaseHelpers.DoWin(transactionInput).Id;
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                baseOutput.Data = new
                {
                    balance = Math.Round(balance, 2),
                    transactionId = transId
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id)
                };
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            catch (Exception ex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException)
                };
                WebApiApplication.DbLogger.Error($"Code: {Constants.Errors.GeneralException} Message: {ex} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
        [HttpPost]
        [Route("{partnerId}/api/pixmove/refund")]
        public HttpResponseMessage Rollback(TransactionInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.sessionId, Constants.DefaultLanguageId, checkExpiration: false);
                if (!int.TryParse(input.playerId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                var client = CacheManager.GetClientById(clientId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var operatorIdKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmovePartnerId);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PixmoveApiKey);
                if (operatorIdKey != input.partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
                var hash = CommonFunctions.ComputeHMACSha256($"refund{input.amount}{client.CurrencyId}{operatorIdKey}{client.Id}" +
                                                             $"{input.roundId}{input.betTransactionId}{clientSession.Token}" +
                                                             $"{input.transactionId}{apiKey}", apiKey).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);


                var doc = BaseHelpers.Rollback(client, input.betTransactionId, input.roundId, ProviderId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, doc?.ProductId ?? 0);

                baseOutput.Data = new
                {
                    balance = Math.Round(balance, 2),
                    transactionId = doc.Id
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id)
                };
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            catch (Exception ex)
            {
                baseOutput.Status = "failed";
                baseOutput.Error = new
                {
                    message = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException)
                };
                WebApiApplication.DbLogger.Error($"Code: {Constants.Errors.GeneralException} Message: {ex} " +
                                                 $"Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}