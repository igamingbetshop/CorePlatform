using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Igromat;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Cache;
using System.Net.Http;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [XmlDeserializer]
    public class IgromatController : ApiController
    {
        [HttpPost]
        [Route("{partnerId}/api/Igromat/ApiRequest")]
        public HttpResponseMessage ApiRequest(int partnerId, HttpRequestMessage request)
        {
            request.Content.Headers.ContentType.MediaType = Constants.HttpContentTypes.ApplicationXml;
            var serializer = new XmlSerializer(typeof(server), new XmlRootAttribute("server"));
            using (Stream stream = request.Content.ReadAsStreamAsync().Result)
            {
                var input = (server)serializer.Deserialize(stream);
                var response = new service
                {
                    session = input.session,
                    time = DateTime.UtcNow.ToString("o")
                };
                try
                {
                    return ProcessApiRequest(input, response);
                }
                catch (FaultException<BllFnErrorType>)
                {
                    using (var baseBl = new BaseBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        return ReturnResponse(response);
                    }
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Error(ex);
                    return ReturnResponse(response);
                }
            }
        }

        private HttpResponseMessage ProcessApiRequest(server input, service response)
        {
            using (var baseBl = new BaseBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                if (input.enter != null && input.enter.Length != 0)
                {
                    response.enter = new serviceEnter[input.enter.Length];
                    for (int i = 0; i < input.enter.Length; i++)
                    {
                        response.enter[i] = Enter(input.enter[i]);
                    }
                }
                if (input.getbalance != null && input.getbalance.Length != 0)
                {
                    response.getbalance = new serviceGetbalance[input.getbalance.Length];
                    for (int i = 0; i < input.getbalance.Length; i++)
                    {
                        response.getbalance[i] = GetBalance(input.getbalance[i]);
                    }
                }
                if (input.roundbet != null && input.roundbet.Length != 0)
                {
                    response.roundbet = new serviceRoundbet[input.roundbet.Length];
                    for (int i = 0; i < input.roundbet.Length; i++)
                    {
                        response.roundbet[i] = RoundBet(input.roundbet[i]);
                    }
                }
                if (input.roundwin != null)
                {
                    response.roundwin = RoundWin(input.roundwin);
                }

                if (input.refund != null)
                {
                    response.refund = Refund(input.refund);
                }

                if (input.logout != null)
                {
                    response.logout = Logout(input.logout);
                }

                return ReturnResponse(response);
            }
        }

        private serviceEnter Enter(serverEnter input)
        {
            var response = new serviceEnter
            {
                id = input.id,
                result = IgromatHelpers.Results.Ok
            };
            try
            {
                using (var baseBl = new BaseBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var session = ClientBll.GetClientProductSession(input.key, Constants.DefaultLanguageId);
                    if (session == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.SessionNotFound);
                    var product = CacheManager.GetProductById(session.ProductId);
                    if (product.ExternalId != input.game.name)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.SessionNotFound);
                    AddTokenDictionary(input.guid, input.key);
                    var client = CacheManager.GetClientById(session.Id);
                    if (client == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                    response.user = new serviceEnterUser
                    {
                        mode = IgromatHelpers.UserModes.Normal,
                        type = IgromatHelpers.UserTypes.Real,
                        wlid = session.Id.ToString()
                    };

                    response.balance = new serviceBalance
                    {
                        currency = client.CurrencyId,
                        value = (int)(balance * 100m),
                        version = 1,//not completed
                        type = IgromatHelpers.CurrencyTypes.Real
                    };
                    response.control = new serviceEnterControl[3];
                    response.control[0] = new serviceEnterControl
                    {
                        stream = IgromatHelpers.Streams.GameData,
                        enable = "true"
                    };
                    response.control[1] = new serviceEnterControl
                    {
                        stream = IgromatHelpers.Streams.Combos,
                        enable = "true"
                    };
                    response.control[2] = new serviceEnterControl
                    {
                        stream = IgromatHelpers.Streams.GameInfo,
                        enable = "true"
                    };
                    return response;
                }
            }
            catch (FaultException<BllFnErrorType>)
            {
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return response;
            }
        }

        private serviceGetbalance GetBalance(serverGetbalance input)
        {
            var response = new serviceGetbalance
            {
                id = input.id,
                result = IgromatHelpers.Results.Ok
            };
            try
            {
                using (var baseBl = new BaseBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    string token = GetTokenFromDictionary(input.guid);
                    var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(session.Id);
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
                    response.balance = new serviceBalance
                    {
                        currency = client.CurrencyId,
                        value = (int)(balance * 100m),
                        version = 1, //not completed
                        type = IgromatHelpers.CurrencyTypes.Real
                    };

                    return response;
                }
            }
            catch (FaultException<BllFnErrorType>)
            {
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return response;
            }
        }

        private serviceRoundbet RoundBet(serverRoundbet input)
        {
            var response = new serviceRoundbet
            {
                id = input.id,
                result = IgromatHelpers.Results.Ok
            };
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        string token = GetTokenFromDictionary(input.guid);
                        var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                        var client = CacheManager.GetClientById(session.Id);
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = session.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.roundnum.id.ToString(),
                            GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Igromat).Id,
                            ExternalOperationId = null,
                            ProductId = session.ProductId,
                            TransactionId = input.id.ToString(),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = Convert.ToDecimal(input.bet) / 100m,
                            DeviceTypeId = session.DeviceType
                        });
                        clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBetLimit(info);

                        var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
                        response.balance = new serviceBalance
                        {
                            currency = client.CurrencyId,
                            value = (int)(balance * 100m),
                            version = 1,
                            type = IgromatHelpers.CurrencyTypes.Real
                        };
                        return response;
                    }
                }
            }
            catch (FaultException<BllFnErrorType>)
            {
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return response;
            }
        }

        private serviceRoundwin RoundWin(serverRoundwin input)
        {
            var response = new serviceRoundwin
            {
                id = input.id,
                result = IgromatHelpers.Results.Ok
            };
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        string token = GetTokenFromDictionary(input.guid);
                        var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                        var client = CacheManager.GetClientById(session.Id);

                        var betDocument = documentBl.GetDocumentByRoundId(
                            (int)OperationTypes.Bet,
                            input.roundnum.id.ToString(),
                            CacheManager.GetGameProviderByName(Constants.GameProviders.Igromat).Id,
                            client.Id
                            );
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var state = (input.win > 0
                            ? (int)BetDocumentStates.Won
                            : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = session.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.roundnum.id.ToString(),
                            GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Igromat).Id,
                            ExternalOperationId = null,
                            ProductId = session.ProductId,
                            TransactionId = input.id.ToString(),
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            Info = JsonConvert.SerializeObject(input.@event),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = Convert.ToDecimal(input.win) / 100m,
                        });
                        clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
                        response.balance = new serviceBalance
                        {
                            currency = client.CurrencyId,
                            value = (int)(balance * 100m),
                            version = 1, //not completed
                            type = IgromatHelpers.CurrencyTypes.Real
                        };
                        return response;
                    }
                }
            }
            catch (FaultException<BllFnErrorType>)
            {
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return response;
            }
        }

        private serviceLogout Logout(serverLogout input)
        {
            var response = new serviceLogout
            {
                id = input.id,
                result = IgromatHelpers.Results.Ok
            };
            try
            {
                using (var baseBl = new BaseBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    string token = GetTokenFromDictionary(input.guid);
                    var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(session.Id);
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
                    DeleteTokenFromDictionary(input.guid);
                    response.balance = new serviceLogoutBalance
                    {
                        currency = client.CurrencyId,
                        value = (int)(balance * 100m),
                        version = 1,//not completed
                        type = IgromatHelpers.CurrencyTypes.Real
                    };
                    return response;
                }
            }
            catch (FaultException<BllFnErrorType>)
            {
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return response;
            }
        }

        private serviceRefund Refund(serverRefund input)
        {
            var response = new serviceRefund
            {
                id = input.id,
                result = IgromatHelpers.Results.Ok
            };
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    string token = GetTokenFromDictionary(input.guid);
                    var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                    var client = CacheManager.GetClientById(session.Id);
                    if (client == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId); // unknown user id

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = session.SessionId,
                        GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Igromat).Id,
                        ExternalOperationId = input.id
                    };

                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
                    response.balance = new serviceBalance
                    {
                        currency = client.CurrencyId,
                        value = (int)(balance * 100m),
                        version = 1, //not completed
                        type = IgromatHelpers.CurrencyTypes.Real
                    };
                    return response;
                }
            }
            catch (FaultException<BllFnErrorType>)
            {
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return response;
            }
        }

        private HttpResponseMessage ReturnResponse(service response)
        {
            var output = CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            return output;
        }

        private static Dictionary<string, string> guidToken = new Dictionary<string,string>();

        private void AddTokenDictionary(string key, string value)
        {
            if (!guidToken.ContainsKey(key))
            {
                guidToken.Add(key, value);
            }
        }

        private string GetTokenFromDictionary(string key)
        {
            if (guidToken.ContainsKey(key))
            {
                return guidToken[key].ToString();
            }
            else
            {
                return key;
            }
        }

        private void DeleteTokenFromDictionary(string key)
        {
            if (guidToken.ContainsKey(key))
            {
                guidToken.Remove(key);
            }
        }
    }
}
