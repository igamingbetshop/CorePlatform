using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.RelaxGaming;
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
using System.IO;
using System.Web;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class RelaxGamingController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.RelaxGaming);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.RelaxGaming).Id;

        [HttpPost]
        [Route("{partnerId}/api/relaxgaming/verifyToken")]
        public HttpResponseMessage VerifyToken(AuthenticationInput input)
        {
            var jsonResponse = "{}";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Debug($" InputString: {bodyStream.ReadToEnd()}");
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var responseObject = new
                    {
                        customerid = client.Id.ToString(),
                        countrycode = clientSession.Country,
                        cashiertoken = clientBl.RefreshClientSession(input.Token, true).Token,
                        customercurrency = client.CurrencyId,
                        balance = Math.Round(balance * 100, 2),
                        jurisdiction = clientSession.Country
                    };
                    jsonResponse = JsonConvert.SerializeObject(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    errormessage = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    errormessage = ex.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            WebApiApplication.DbLogger.Debug("Response: " + jsonResponse);
            httpResponseMessage.Content = new StringContent(jsonResponse, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/relaxgaming/getBalance")]
        public HttpResponseMessage GetBalance(BaseInput input)
        {
            var jsonResponse = "{}";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Debug($" InputString: {bodyStream.ReadToEnd()}");
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                var responseObject = new
                {
                    customerid = client.Id.ToString(),
                    customercurrency = client.CurrencyId,
                    balance = Math.Round(balance * 100, 2)
                };
                jsonResponse = JsonConvert.SerializeObject(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    errormessage = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    errormessage = ex.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            WebApiApplication.DbLogger.Debug("Response: " + jsonResponse);
            httpResponseMessage.Content = new StringContent(jsonResponse, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/relaxgaming/withdraw")]
        [Route("{partnerId}/api/relaxgaming/deposit")]
        public HttpResponseMessage DoTransaction(TransactionInput input)
        {
            var jsonResponse = "{}";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Debug($" InputString: {bodyStream.ReadToEnd()}");
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId ||  product.ExternalId != input.GameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                long transId = 0;
                var transactionInput = new Models.BaseModels.TransactionInput
                {
                    Client = client,
                    ProviderId = ProviderId,
                    ProductExternalId = input.GameId,
                    SessionId = clientSession.SessionId,
                    SessionParentId = clientSession.ParentId ?? 0,
                    SessionDeviceType = clientSession.DeviceType,
                    Amount = input.Amount / 100,
                    TransactionId = input.TransactionId,
                    RoundId = input.RoundId,
                    IsRoundClosed = input.CloseRound
                };
                if (input.TransactionType == "withdraw")
                    transId = BaseHelpers.DoBet(transactionInput).Id;
                else if (input.TransactionType == "deposit" || input.TransactionType == "freespinspayout" ||
                        input.TransactionType == "fsdeposit" || input.TransactionType=="promopayout")
                {
                    transactionInput.IsFreeSpin = input.TransactionType == "freespinspayout" || input.TransactionType == "fsdeposit" ||
                                                  input.TransactionType=="promopayout";
                    transId = BaseHelpers.DoWin(transactionInput).Id;
                }
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                var responseObject = new
                {
                    balance = Math.Round(balance * 100, 2),
                    txid = input.TransactionId,
                    remotetxid = transId
                };
                jsonResponse = JsonConvert.SerializeObject(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    errormessage = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    errormessage = ex.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            WebApiApplication.DbLogger.Debug("Response: " + jsonResponse);
            httpResponseMessage.Content = new StringContent(jsonResponse, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/relaxgaming/rollback")]
        public HttpResponseMessage Rollback(RollbackInput input)
        {
            var jsonResponse = "{}";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Debug($" InputString: {bodyStream.ReadToEnd()}");
                if (!int.TryParse(input.ClientId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                var client = CacheManager.GetClientById(clientId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var doc = BaseHelpers.Rollback(client, input.OriginalTransactionId, input.RoundId, ProviderId);
                if (doc ==null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, doc?.ProductId ?? 0);

                var responseObject = new
                {
                    balance = Math.Round(balance * 100, 2),
                    txid = input.TransactionId,
                    remotetxid = doc?.Id ?? 0
                };
                jsonResponse = JsonConvert.SerializeObject(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    errormessage = fex.Detail.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
                jsonResponse = JsonConvert.SerializeObject(new
                {
                    errorcode = RelaxGamingHelpers.GetErrorMsg(Constants.Errors.GeneralException),
                    errormessage = ex.Message
                });
                httpResponseMessage.StatusCode = RelaxGamingHelpers.GetErrorCode(Constants.Errors.GeneralException);
            }
            WebApiApplication.DbLogger.Debug("Response: " + jsonResponse);
            httpResponseMessage.Content = new StringContent(jsonResponse, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}