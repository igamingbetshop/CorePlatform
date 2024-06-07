using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.ESport;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using System.Linq;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class ESportController : ApiController
    {
        private static List<string> WhitelistedIps = new List<string>
        {
            "?????"
        };
        private static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.ESport).Id;
        private static BllProduct Product = CacheManager.GetProductByExternalId(ProviderId, "ESport");

        [HttpPost]
        [Route("{partnerId}/api/ESport/Authentication")]
        public HttpResponseMessage Authorization(int partnerId, BaseInput input)
        {
            var jsonResponse = string.Empty;
            var response = new BaseOutput
            {
                Result = true
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("input: " + JsonConvert.SerializeObject(input));
                var auth = HttpContext.Current.Request.Headers.Get("Authorization");
                if(auth==null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                WebApiApplication.DbLogger.Info("Got auth: " + auth);

                auth = auth.Replace("Bearer", string.Empty).Trim();
                var signString = JsonConvert.SerializeObject(input,
                                                       new JsonSerializerSettings()
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
                var sec = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.ESportSecureKey);
                signString = CommonFunctions.ComputeHMACSha512(signString, sec);
                if (auth.ToLower() != signString.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
             /*   var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);*/
                response.PlayerId = client.Id;
                response.Username = string.Format("{0} {1}", client.FirstName, client.LastName);
                response.Currency = client.CurrencyId;
                response.Balance = BaseHelpers.GetClientProductBalance(client.Id, Product.Id);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.Result = false;
                if (fex.Detail != null)
                {
                    response.Error = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.Error = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Error = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response,
                                                       new JsonSerializerSettings()
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
            WebApiApplication.DbLogger.Info("Output=" + jsonResponse);
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/ESport/GetWallet")]
        public HttpResponseMessage GetBalance(int partnerId, BaseInput input)
        {
            var jsonResponse = string.Empty;
            var response = new BaseOutput
            {
                Result = true
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var auth = HttpContext.Current.Request.Headers.Get("Authorization");
                if (auth == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                auth = auth.Replace("Bearer", string.Empty).Trim();
                var signString = JsonConvert.SerializeObject(input,
                                                       new JsonSerializerSettings()
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
                signString = CommonFunctions.ComputeHMACSha512(signString, CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.ESportSecureKey));
                if (auth.ToLower() != signString.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(input.PlayerId.Value);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response.PlayerId = client.Id;
                response.Currency = client.CurrencyId;
                response.Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.Result = false;
                if (fex.Detail != null)
                {
                    response.Error = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.Error = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Error = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response,
                                                       new JsonSerializerSettings()
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
            WebApiApplication.DbLogger.Info("Output=" + jsonResponse);
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/ESport/UpdateWallet")]
        public HttpResponseMessage UpdateWallet(int partnerId, BetInput input)
        {
            var jsonResponse = string.Empty;
            var response = new BetOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("input: " + JsonConvert.SerializeObject(input));
                var auth = HttpContext.Current.Request.Headers.Get("Authorization");
                if (auth == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                WebApiApplication.DbLogger.Info("Got auth: " + auth);
                auth = auth.Replace("Bearer", string.Empty).Trim();
                
                var signString = JsonConvert.SerializeObject(input,
                                                       new JsonSerializerSettings()
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });

                signString = CommonFunctions.ComputeHMACSha512(signString, CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.ESportSecureKey));
                if (auth.ToLower() != signString.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                    Product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductBlockedForThisPartner);
                foreach (var inputItem in input.Data)
                {
                    switch (inputItem.Action.ToLower())
                    {
                        case ESportHelpers.Actions.Bet:
                        case ESportHelpers.Actions.FantasyPool:
                            response.Results.Add(DoBet(inputItem, partnerId));
                            break;
                        case ESportHelpers.Actions.WonBet:
                        case ESportHelpers.Actions.Lost:
                            response.Results.Add(DoWin(inputItem, partnerId));
                            break;
                        case ESportHelpers.Actions.BetRollback:
                        case ESportHelpers.Actions.WinRefund:
                            response.Results.Add(Rollback(inputItem, partnerId));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.Error = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.Error = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.Error = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response,
                                                       new JsonSerializerSettings()
                                                       {
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
            WebApiApplication.DbLogger.Info("Output=" + jsonResponse);
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
        }

        private BetOutputItem DoBet(BetItem input, int partnerId)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                           Product.Id);
                        var client = CacheManager.GetClientById(Convert.ToInt32(input.User_Id));
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var document = documentBl.GetDocumentByExternalId(input.Wallet_Id, client.Id,
                                ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ExternalProductId = Product.ExternalId,
                                ProductId = Product.Id,
                                TransactionId = input.Wallet_Id,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = Math.Abs(input.Amount)

                            });
                            document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        return new BetOutputItem
                        {
                            Wallet_Id = input.Wallet_Id,
                            Result = true,
                            Transaction_Id = document.Id.ToString(),
                            Currency = client.CurrencyId,
                            Balance = BaseHelpers.GetClientProductBalance(client.Id, Product.Id)
                        };
                    }
                }
            }
            catch
            {
                return new BetOutputItem
                {
                    Result = false
                };
            }
        }

        private BetOutputItem DoWin(BetItem input, int partnerId)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                          Product.Id);
                        var client = CacheManager.GetClientById(Convert.ToInt32(input.User_Id));
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(input.Bet_Id, client.Id,
                                ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.Wallet_Id, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var state = input.Action == ESportHelpers.Actions.WonBet ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = Product.ExternalId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.Wallet_Id,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = Math.Abs(input.Amount)
                            });

                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl).FirstOrDefault();
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        }

                        return new BetOutputItem
                        {
                            Wallet_Id = input.Wallet_Id,
                            Result = true,
                            Transaction_Id = winDocument.Id.ToString(),
                            Currency = client.CurrencyId,
                            Balance = BaseHelpers.GetClientProductBalance(client.Id, Product.Id)
                        };
                    }
                }
            }
            catch
            {
                return new BetOutputItem
                {
                    Result = false
                };
            }
        }

        private BetOutputItem Rollback(BetItem input, int partnerId)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                          Product.Id);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.User_Id));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var document = documentBl.GetDocumentByExternalId(input.Bet_Id, client.Id,
                            ProviderId, partnerProductSetting.Id,
                            input.Action == ESportHelpers.Actions.BetRollback ? (int)OperationTypes.Bet : (int)OperationTypes.Win);

                    if (document == null)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    }
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        TransactionId = input.Bet_Id,
                        ProductId = Product.Id
                    };
                    if (document.State != (int)BetDocumentStates.Deleted)
                    {
                        document = documentBl.RollbackProductTransactions(operationsFromProduct).FirstOrDefault();
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    }

                        return new BetOutputItem
                    {
                        Wallet_Id = input.Wallet_Id,
                        Result = true,
                        Transaction_Id = document.Id.ToString(),
                        Currency = client.CurrencyId,
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, Product.Id)
                    };
                }
            }
            catch
            {
                return new BetOutputItem
                {
                    Result = false
                };
            }
        }
    }
}
