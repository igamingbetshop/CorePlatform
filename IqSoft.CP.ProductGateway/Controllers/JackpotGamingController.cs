using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.JackpotGaming;
using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Services;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using System.Web;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Text;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class JackpotGamingController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.JackpotGaming).Id;

        [HttpGet]
        [Route("{partnerId}/api/JackpotGaming/wallets")]
        public HttpResponseMessage Authenticate([FromUri] BalanceInput input)
        {
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var response = string.Empty;
            try
            {
                if (!Request.Headers.Contains("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = HttpContext.Current.Request.Headers.Get("token");
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                response = JsonConvert.SerializeObject(new BalanceOutput()
                {
                    UserId = client.Id.ToString(),
                    Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId),
                    Currency = client.CurrencyId,
                    WalletId = client.Id.ToString(),
                    Data = new Data
                    {
                        User = new User
                        {
                            Username = client.UserName,
                            Name = client.FirstName ?? client.UserName,
                            Lastname = client.LastName ?? client.UserName,
                            Email = client.Email,
                            Status = client.State == (int)ClientStates.Active
                        }
                    }
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response =  JsonConvert.SerializeObject(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            WebApiApplication.DbLogger.Info(response);
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/JackpotGaming/wallet/{walletId}/debit")]
        public HttpResponseMessage Debit(DebitInput input)
        {
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var response = string.Empty;

            try
            {
                WebApiApplication.DbLogger.Info($"Input: " + JsonConvert.SerializeObject(input));
                if (!Request.Headers.Contains("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = HttpContext.Current.Request.Headers.Get("token");
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.Details.Request.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Details.Request.AmountCurrency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.Details.Request.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                if (product.Id != clientSession.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (input.TransactionType.ToUpper() == "ROLLBACK")
                {
                    var docId = RollbackTransaction(input, product.Id, client, OperationTypes.Bet);
                    response = JsonConvert.SerializeObject(new DebitOutput
                    {
                        TransactionId = docId,
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                        Currency = client.CurrencyId,
                        Debit = input.Details.Request.Amount,
                        Message = "debited"
                    });

                }
                else if (input.TransactionType.ToUpper() == "BET")
                {
                    using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(documentBl))
                        {
                            var document = documentBl.GetDocumentByExternalId(input.Details.Request.TransactionId, client.Id, ProviderId,
                                                                              partnerProductSetting.Id, (int)OperationTypes.Bet);
                            if (document != null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.Reference,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.Details.Request.TransactionId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Details.Request.Amount,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);

                            response = JsonConvert.SerializeObject(new DebitOutput
                            {
                                TransactionId = document.Id.ToString(),
                                Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                                Currency = client.CurrencyId,
                                Debit = input.Details.Request.Amount,
                                Message = "debited"
                            });
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response =  JsonConvert.SerializeObject(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            WebApiApplication.DbLogger.Info(response);
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            return httpResponseMessage;
        }


        [HttpPost]
        [Route("{partnerId}/api/JackpotGaming/wallet/{walletId}/credit")]
        public HttpResponseMessage Credit(DebitInput input)
        {
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var response = string.Empty;
            try
            {
                WebApiApplication.DbLogger.Info($"Input: " + JsonConvert.SerializeObject(input));
                if (!Request.Headers.Contains("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = HttpContext.Current.Request.Headers.Get("token");
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.Details.Request.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Details.Request.AmountCurrency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.Details.Request.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                if (product.Id != clientSession.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (input.TransactionType.ToUpper() == "ROLLBACK")
                {
                    var docId = RollbackTransaction(input, product.Id, client, OperationTypes.Bet);
                    response = JsonConvert.SerializeObject(new DebitOutput
                    {
                        TransactionId = docId,
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                        Currency = client.CurrencyId,
                        Debit = input.Details.Request.Amount,
                        Message = "debited"
                    });

                }
                else if (input.TransactionType.ToUpper() == "WIN")
                {
                    using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(documentBl))
                        {
                            var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.Reference, ProviderId,
                                                                             client.Id, (int)BetDocumentStates.Uncalculated);
                            if (betDocument == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                            var winDocument = documentBl.GetDocumentByExternalId(input.Details.Request.TransactionId, client.Id, ProviderId,
                                                                                 partnerProductSetting.Id, (int)OperationTypes.Win);
                            if (winDocument != null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentAlreadyWinned);

                            var state = input.Details.Request.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                RoundId = input.Reference,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.Details.Request.TransactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Details.Request.Amount
                            });
                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];

                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = input.Details.Request.Amount,
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                            response = JsonConvert.SerializeObject(new DebitOutput()
                            {
                                TransactionId = winDocument.Id.ToString(),
                                Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                                Currency = client.CurrencyId,
                                Credit = Convert.ToDecimal(input.Amount),
                                Message = "credited"
                            });
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response =  JsonConvert.SerializeObject(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            WebApiApplication.DbLogger.Info(response);
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            return httpResponseMessage;
        }


        [HttpPost]
        [Route("{partnerId}/api/JackpotGaming/status")]
        public HttpResponseMessage CloseRound(CloseRoundInput input)
        {
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var response = string.Empty;
            try
            {
                if (!Request.Headers.Contains("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = HttpContext.Current.Request.Headers.Get("token");
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId || product.ExternalId != input.GameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);

                if (input.RoundClose)
                {
                    using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId,
                                                                            client.Id, (int)BetDocumentStates.Uncalculated);
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            foreach (var betDoc in betDocuments)
                            {
                                betDoc.State = (int)BetDocumentStates.Lost;
                                listOfOperationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                                listOfOperationsFromApi.CreditTransactionId = betDoc.Id;
                                var doc = clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl);
                            }
                        }
                        response = JsonConvert.SerializeObject(new DebitOutput()
                        {
                            TransactionId = "0",
                            Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                            Currency = client.CurrencyId,
                            Message = "status"
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response =  JsonConvert.SerializeObject(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }
            WebApiApplication.DbLogger.Info(response);
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            return httpResponseMessage;
        }

        private string RollbackTransaction(DebitInput input, int productId, BllClient client, OperationTypes operationType)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var product = CacheManager.GetProductById(productId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = input.Details.Request.TransactionId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id,
                    OperationTypeId = (int)operationType
                };
                try
                {
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    return doc[0].Id.ToString();
                }
                catch (FaultException<BllFnErrorType> fex)
                {

                    if (fex.Detail.Id != (int)Constants.Errors.DocumentAlreadyRollbacked)
                        throw;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    var document = documentBl.GetDocumentByExternalId(input.Reference, client.PartnerId, ProviderId,
                                                                      partnerProductSetting.Id, (int)operationType);
                    return document.Id.ToString();
                }
            }
        }
    }
}