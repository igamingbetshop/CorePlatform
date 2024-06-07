using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.SkyCity;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Enums;
using System.Text;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class SkyCityController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.SkyCity);
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SkyCity).Id;

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/balance")]
        public HttpResponseMessage GetBalance(int partnerId, BaseInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientByUserName(partnerId, input.UserId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                response = new BaseOutput
                {
                    Result = SkyCityHelpers.ErrorCode.Normal,
                    Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, 0) * 100).ToString("0.##########"))
                };
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        // roiki Nov9 =================================================
        private void CheckSecureKey(int partnerId, BaseInput input)
        {
            var inputSign = HttpContext.Current.Request.Headers.Get("key");
            var secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SkyCitySecureKey);

            WebApiApplication.DbLogger.Info("Provider ID : " + ProviderId);
            if (string.IsNullOrEmpty(secureKey))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            WebApiApplication.DbLogger.Info("reqbody: " + bodyText);
            WebApiApplication.DbLogger.Info(secureKey);

            var jsonMessage = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            WebApiApplication.DbLogger.Info(jsonMessage);
            //var sign = CommonFunctions.ComputeSha256(jsonMessage+secureKey);
            //var sign = CommonFunctions.ComputeSha256("a"+secureKey);
            var sign = CommonFunctions.ComputeHMACSha256(bodyText, secureKey);

            WebApiApplication.DbLogger.Info("sign to lower : " + sign.ToLower());
            WebApiApplication.DbLogger.Info("inputsign.tolower : " + inputSign.ToLower());

            if (sign.ToLower() != inputSign.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/betting")]
        [Route("{partnerId}/api/SkyCity/addbetting")]
        public HttpResponseMessage DoBet(int partnerId, BetInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameCode.ToString());
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                            product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var client = CacheManager.GetClientByUserName(partnerId, input.UserId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var document = documentBl.GetDocumentByExternalId(input.Bet.ExternalTransactionId.ToString(),
                            client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                ExternalProductId = input.GameCode.ToString(),
                                ProductId = partnerProductSetting.ProductId,
                                TransactionId = input.Bet.ExternalTransactionId.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Bet.Amount
                            });
							clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        response = new BaseOutput
                        {
                            Result = SkyCityHelpers.ErrorCode.Normal,
                            Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0.##########"))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/prize")]
        public HttpResponseMessage DoWin(int partnerId, BetInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameCode.ToString());
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                            product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var client = CacheManager.GetClientByUserName(partnerId, input.UserId);

                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId,
                            ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.Bet.ExternalTransactionId.ToString(),
                            client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var state = (input.Bet.WinAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.GameCode.ToString(),
                                ProductId = betDocument.ProductId,
                                TransactionId = input.Bet.ExternalTransactionId.ToString(),
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Bet.WinAmount
                            });
							clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        }
                        response = new BaseOutput
                        {
                            Result = SkyCityHelpers.ErrorCode.Normal,
                            Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0.##########"))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/cancel")]
        public HttpResponseMessage Rollback(int partnerId, BetInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var product = CacheManager.GetProductByExternalId(ProviderId, input.GameCode.ToString());
					var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
						product.Id);
					if (partnerProductSetting == null || partnerProductSetting.Id == 0)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
					var client = CacheManager.GetClientByUserName(partnerId, input.UserId);
					if (client == null)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
					var operationsFromProduct = new ListOfOperationsFromApi
					{
						GameProviderId = ProviderId,
						TransactionId = input.Bet.ExternalTransactionId.ToString(),
						ProductId = partnerProductSetting.ProductId
					};
					var betDocument =
						documentBl.GetDocumentByExternalId(input.Bet.ExternalTransactionId.ToString(),
						client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
					if (betDocument == null)
					{
						WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
						throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
					}
                    if (betDocument.State != (int)BetDocumentStates.Deleted)
                    {
                        documentBl.RollbackProductTransactions(operationsFromProduct);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    }
                        response = new BaseOutput
					{
						Result = SkyCityHelpers.ErrorCode.Normal,
						Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0.##########"))
					};
				}
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
            httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
            return httpResponse;
        }
    }
}
