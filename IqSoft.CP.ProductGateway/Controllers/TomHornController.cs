using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.TomHorn;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using System.Text;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class TomHornController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TomHorn).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.TomHorn);
        [HttpPost]
        [Route("{partnerId}/api/TomHorn/GetBalance")]
        public HttpResponseMessage GetBalance(BalanceInput input)
        {
            var output = new BalanceOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                //CheckSign(input, client.PartnerId, input.Sign, input.OperatorId, false);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                output.Balance = new Balance
                {
                    Amount = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, 0))),
                    Currency = client.CurrencyId
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var jsonResponse = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/TomHorn/Withdraw")]
        public HttpResponseMessage DoBet(BetInput input)
        {
            var output = new BetOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameModule);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                CheckSign(input, client.PartnerId, input.Sign, product.SubProviderId ?? product.GameProviderId.Value);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        if (input.Currency != client.CurrencyId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        var clientSession = ClientBll.GetClientProductSession(input.SessionId.ToString(), Constants.DefaultLanguageId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id,
                                                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                RoundId = input.GameRoundId.HasValue ? input.GameRoundId.Value.ToString() : string.Empty,
                                ExternalProductId = input.GameModule,
                                ProductId = product.Id,
                                TransactionId = input.TransactionId.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        output.Transaction = new Transaction
                        {
                            Id = betDocument.Id,
                            Currency = client.CurrencyId,
                            Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var jsonResponse = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/TomHorn/Deposit")]
        public HttpResponseMessage DoWin(BetInput input)
        {
            var output = new BetOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameModule);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                CheckSign(input, client.PartnerId, input.Sign, product.SubProviderId ?? product.GameProviderId.Value);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        if (input.Currency != client.CurrencyId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        var clientSession = ClientBll.GetClientProductSession(input.SessionId.ToString(), Constants.DefaultLanguageId, null, false);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.GameRoundId.ToString(), ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId.Value.ToString(), client.Id,
                                                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.GameRoundId.ToString(),
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.GameModule.ToString(),
                                ProductId = betDocument.ProductId,
                                TransactionId = input.TransactionId.ToString(),
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount

                            });
                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                BetId = betDocument?.Id ?? 0,
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = input.Amount,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        output.Transaction = new Transaction
                        {
                            Id = winDocument.Id,
                            Currency = client.CurrencyId,
                            Balance = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var jsonResponse = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("{partnerId}/api/TomHorn/RollbackTransaction")]
        public HttpResponseMessage RollbackTransaction(RollbackInput input)
        {
            var output = new BetOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    BaseBll.CheckIp(WhitelistedIps);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var clientSession = ClientBll.GetClientProductSession(input.SessionId.ToString(), Constants.DefaultLanguageId, null, false);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    CheckSign(input, client.PartnerId, input.Sign, product.SubProviderId ?? product.GameProviderId.Value);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Bet,
                        TransactionId = input.TransactionId.Value.ToString()
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct, false);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var jsonResponse = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse, Encoding.UTF8) };
        }

        private void CheckSign(object input, int partnerId, string inputSign, int subProviderId)
        {
            string partnerKey, salt;
            if (subProviderId == ProviderId)
            {
                partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TomHornOperatorId);
                salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TomHornSecretKey);
            }
            else
            {
                partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TomHorn3OperatorId);
                salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TomHorn3SecretKey);
            }
            var paramsValues = new List<string>();
            var properties = input.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(input, null);
                if (field.Name.ToLower().Contains("sign") || value == null || string.IsNullOrEmpty(value.ToString()))
                    continue;
                paramsValues.Add(value.ToString());
            }
            var sign = paramsValues.Aggregate(string.Empty, (current, par) => current + par);
            sign = CommonFunctions.ComputeHMACSha256(sign, salt).ToUpperInvariant();
            if (inputSign != sign)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }
    }
}