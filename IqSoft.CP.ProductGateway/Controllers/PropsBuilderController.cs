using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.PropsBuilder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class PropsBuilderController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.PropsBuilder).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.PropsBuilder);

        [HttpPost]
        [Route("{partnerId}/api/PropsBuilder/login")]
        [Route("{partnerId}/api/PropsBuilder/keep-alive")]
        public HttpResponseMessage Authenticate()
        {
            var inputString = string.Empty;
            var output = new BaseOutput { Status = "ok", TimeOut = 25 };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                WebApiApplication.DbLogger.Info("Input:" + inputString);
                var input = JsonConvert.DeserializeObject<BaseInput>(inputString);

                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PropsBuilderApiKey);
                inputString = inputString.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                inputString = inputString.Substring(0, inputString.IndexOf(",\"signature\""))+ "}";
                if (input.signature.ToLower() != CommonFunctions.ComputeHMACSha1(inputString, apiKey).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (!string.IsNullOrEmpty(input.user) && input.user != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidUserName);
                output.AgentsInfo = new List<AgentInfo>();
                output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId)));

                if (client.UserId.HasValue)
                {
                    var agent = CacheManager.GetUserById(client.UserId.Value);
                    var parentAgent = agent.Id;
                    var path = agent.Path.Split('/').Where(x=> !string.IsNullOrEmpty(x)).ToList();
                    for (int i = 0; i < path.Count - 1; ++i)
                    {
                        var agentId = Convert.ToInt32(path[i]);
                        var child = CacheManager.GetUserById(agentId);
                        output.AgentsInfo.Add(new AgentInfo { AgentId = parentAgent.ToString(), MasterAgentId = path[i], Level = child.Level.Value});
                        parentAgent = agentId;
                    }
                }
                else
                {
                    output.AgentsInfo.Add(new AgentInfo { AgentId = "0", MasterAgentId ="0", Level = 0 });
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Status = "error";
                output.ErrorDescription = fex.Detail.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Status = "error";
                output.ErrorDescription = ex.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/PropsBuilder/debit")]
        public HttpResponseMessage PlaceBet()
        {
            var inputString = string.Empty;
            var output = new BaseOutput { Status = "ok" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                WebApiApplication.DbLogger.Info("Input:" + inputString);
                var input = JsonConvert.DeserializeObject<BetInput>(inputString);

                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PropsBuilderApiKey);
                inputString = inputString.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                inputString = inputString.Substring(0, inputString.IndexOf(",\"signature\"")) + "}";
                if (input.signature.ToLower() != CommonFunctions.ComputeHMACSha1(inputString, apiKey).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.user  != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidUserName);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var document = documentBl.GetDocumentByExternalId(input.BetId, client.Id, ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            RoundId = input.BetId,
                            TransactionId = input.BetId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = Math.Abs(input.Amount),
                            DeviceTypeId = clientSession.DeviceType
                        });
                        document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId)));
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Status = "error";
                output.ErrorDescription = fex.Detail.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Status = "error";
                output.ErrorDescription = ex.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/PropsBuilder/credit")]
        [Route("{partnerId}/api/PropsBuilder/lost")]
        public HttpResponseMessage SettleBet()
        {
            var inputString = string.Empty;
            var output = new BaseOutput { Status = "ok" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                WebApiApplication.DbLogger.Info("Input:" + inputString);
                var input = JsonConvert.DeserializeObject<BetInput>(inputString);

                var client = CacheManager.GetClientById(Convert.ToInt32(input.user));
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PropsBuilderApiKey);
                inputString = inputString.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                inputString = inputString.Substring(0, inputString.IndexOf(",\"signature\"")) + "}";
                if (input.signature.ToLower() != CommonFunctions.ComputeHMACSha1(inputString, apiKey).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.user  != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidUserName);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, Constants.GameProviders.PropsBuilder);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var betDocument = documentBl.GetDocumentByExternalId(input.BetId, client.Id, ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);

                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                     partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.BetId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = product.ExternalId,
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
                            Amount = input.Amount,
                            DeviceTypeId = betDocument.DeviceTypeId
                        });
                        clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        if (input.Amount >0)
                        {
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = input.Amount,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                    }
                    output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)));
                }

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Status = "error";
                output.ErrorDescription = fex.Detail.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Status = "error";
                output.ErrorDescription = ex.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/PropsBuilder/void")]
        public HttpResponseMessage RollBackTransaction()
        {
            var inputString = string.Empty;
            var output = new BaseOutput { Status = "ok" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                WebApiApplication.DbLogger.Info("Input:" + inputString);
                var input = JsonConvert.DeserializeObject<BetInput>(inputString);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.user));
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PropsBuilderApiKey);
                inputString = inputString.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                inputString = inputString.Substring(0, inputString.IndexOf(",\"signature\"")) + "}";
                if (input.signature.ToLower() != CommonFunctions.ComputeHMACSha1(inputString, apiKey).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, Constants.GameProviders.PropsBuilder);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        TransactionId = input.BetId,
                        ProductId = product.Id
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)));
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Status = "error";
                output.ErrorDescription = fex.Detail.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Status = "error";
                output.ErrorDescription = ex.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/PropsBuilder/undo")]
        public HttpResponseMessage Resettle()
        {
            var inputString = string.Empty;
            var output = new BaseOutput { Status = "ok" };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    inputString = reader.ReadToEnd();
                }
                WebApiApplication.DbLogger.Info("Input:" + inputString);
                var input = JsonConvert.DeserializeObject<BetInput>(inputString);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.user));
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PropsBuilderApiKey);
                inputString = inputString.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                inputString = inputString.Substring(0, inputString.IndexOf(",\"signature\"")) + "}";
                if (input.signature.ToLower() != CommonFunctions.ComputeHMACSha1(inputString, apiKey).ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, Constants.GameProviders.PropsBuilder);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(input.BetId, client.Id, ProviderId,
                                                                     partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);

                    var winDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Win, input.BetId, ProviderId,
                                                                       client.Id, (int)BetDocumentStates.Won);
                    winDocuments.ForEach(doc =>
                    {
                        var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            GameProviderId = ProviderId,
                            TransactionId = doc.ExternalTransactionId,
                            OperationTypeId = (int)OperationTypes.Win
                        };
                        documentBl.RollbackProductTransactions(rollbackOperationsFromProduct, false, input.TransactionId);
                    });                   
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    output.Balance  = decimal.Parse(string.Format("{0:N2}", BaseHelpers.GetClientProductBalance(client.Id, product.Id)));
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Status = "error";
                output.ErrorDescription = fex.Detail.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Status = "error";
                output.ErrorDescription = ex.Message;
                WebApiApplication.DbLogger.Error("Input: " + inputString + "_" + ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }
    }
}
