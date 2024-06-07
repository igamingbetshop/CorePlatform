using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.BetSolutions;
using System.Net.Http;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.ServiceModel;
using System.Net;
using System.Net.Http.Headers;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;
using System.Linq;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class BetSolutionsController : ApiController
    {
        private readonly static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetSolutions).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.BetSolutions);

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/Authentication")]
        public HttpResponseMessage Authentication(AuthenticationInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.PublicToken, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", input.Hash, key));
                //if (hash.ToLower() != input.Hash.ToLower())
                //    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    response.Data = new { PrivateToken = clientBl.RefreshClientSession(input.PublicToken, true).Token };
                }


            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            var ss = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ss)
            };
            WebApiApplication.DbLogger.Info("Output:" + ss);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/GetBalance")]
        public HttpResponseMessage GetBalance(BaseInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", CommonFunctions.GetSortedValuesAsString(input, "|"), key));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                response.Data = new { CurrentBalance = Math.Truncate(BaseHelpers.GetClientProductBalance(client.Id, 0) * 100) };

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }
        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/Bet")]
        public HttpResponseMessage DoBet(BetInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", CommonFunctions.GetSortedValuesAsString(input, "|"), key));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id, ProviderId,
                                                                          partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (document == null)
                        {
                            if (input.BetTypeId == (int)BetSolutionsHelpers.TransactionsTypes.Normal)
                            {
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.SessionId,
                                    CurrencyId = client.CurrencyId,
                                    GameProviderId = ProviderId,
                                    ProductId = product.Id,
                                    RoundId = input.RoundId,
                                    TransactionId = input.TransactionId,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = input.Amount / 100m,
                                    DeviceTypeId = clientSession.DeviceType
                                });
                                document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                                BaseHelpers.BroadcastBetLimit(info);
                                response.Data = new
                                {
                                    CurrentBalance = Math.Truncate(BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100),
                                    TransactionId = document.Id
                                };
                            }
                        }
                        response.Data = new { CurrentBalance = Math.Truncate(BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100) };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSolutions/Win")]
        public HttpResponseMessage DoWin(BetInput input)
        {
            var response = new BaseOutput { StatusCode = (int)BetSolutionsHelpers.StatusCodes.Success };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSolutionsSecureKey);
                var hash = input.Hash;
                input.Hash = null;
                input.Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", CommonFunctions.GetSortedValuesAsString(input, "|"), key));
                if (hash.ToLower() != input.Hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        if (betDocument.ProductId != product.Id)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongProductId);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null && input.WinTypeId == (int)BetSolutionsHelpers.TransactionsTypes.Normal)
                        {
                            var amount = input.Amount / 100m;
                            var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.GameId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.TransactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
                            });

                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            response.Data = new
                            {
                                CurrentBalance = Math.Truncate(BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100),
                                TransactionId = winDocument.Id
                            };
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = Convert.ToDecimal(amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }

                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.StatusCode = BetSolutionsHelpers.GetErrorCode(Constants.Errors.GeneralException);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }
    }
}