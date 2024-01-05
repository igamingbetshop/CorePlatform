using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.ServiceModel;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.ZeusPlay;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;

using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class ZeusPlayController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.ZeusPlay).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "193.70.63.197"
        };

        [HttpPost]
        [Route("{partnerId}/api/ZeusPlay/ApiRequest")]
        public ActionResult ApiRequest(InputBase input)
        {
            var output = "OK";

            try
            {
                try
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                }
                catch
                {
                    Program.DbLogger.Info("ZeusPlay NewIp: " + Request.Headers["CF-Connecting-IP"]);
                }
                switch (input.Func)
                {
                    case ZeusPlayHelpers.Methods.Authenticate:
                        output = Authenticate(input);
                        break;
                    case ZeusPlayHelpers.Methods.GetBalance:
                        output = GetBalance(input);
                        break;
                    case ZeusPlayHelpers.Methods.DoBet:
                        output = DoBet(input);
                        break;
                    case ZeusPlayHelpers.Methods.DoWin:
                        output = DoWin(input);
                        break;
                    case ZeusPlayHelpers.Methods.GameSessionEnd:
                          output = GameSessionEnd(input);
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "Error: " + errorMessage);

                output = string.Format("ERROR|{0}", errorMessage);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "Error: " + ex);
                output = string.Format("ERROR|{0}", Constants.Errors.GeneralException);
            }
            return Ok(output);
        }

       /* [HttpPost]
        [Route("{partnerId}/api/ZeusPlay/Reports")]
        public ActionResult Reports(int partnerId, HttpRequestMessage request)
        {
			var serializer = new XmlSerializer(typeof(Report), new XmlRootAttribute("report"));
            var byteArray = request.Content.ReadAsByteArrayAsync().Result;

            Report input;
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            using (var reader = new StringReader(responseString))
            {
                input = (Report)serializer.Deserialize(reader);
            }
            Program.DbLogger.Info("Reposrts input: " + JsonConvert.SerializeObject(input));
            string output = "";
            try
            {
                for (int i = 0; i < input.Item.Session.Length; i++)
                {
                    var openSessions = input.Item.Session;
                    string message = string.Format("session{0}{1}{2}{3}{4}{5}{6}{7}", openSessions[i].Id, openSessions[i].PlayerId,
                        openSessions[i].SumBets, openSessions[i].SumWins, openSessions[i].CountBets, openSessions[i].CountWins,
                        openSessions[i].Currency, input.Item.Random);
                    CheckSign(message, openSessions[i].Datasig, partnerId);
                    CheckSession(openSessions[i].Id, false);
                }
                output = "OK";
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(faultException));
                var errorMessage = faultException.Detail == null ? Constants.Errors.GeneralException : faultException.Detail.Id;
                output = string.Format("ERROR|{0}", errorMessage);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                output = string.Format("ERROR|{0}", Constants.Errors.GeneralException);
            }
            var response = new ActionResult()
            {
                Content = new StringContent(output, Encoding.UTF8)
            };
            return response;
        }*/

        private static string Authenticate(InputBase input)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var key = CheckSign(client.PartnerId, string.Format("{0}{1}", input.Func, input.Token), input.DataSignature);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            var output = string.Format("OK|{0}|{1}|", client.Id, product.ExternalId);
            return string.Format("{0}{1}", output, CommonFunctions.ComputeMd5(output.Replace("|", string.Empty) + key));
        }

        private static string GetBalance(InputBase input)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var key = CheckSign(client.PartnerId, string.Format("{0}{1}{2}", input.Func, input.Token, client.CurrencyId), input.DataSignature);
            var balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
            var output = string.Format("OK|{0}|{1}|{2}|", clientSession.Token, balance, client.CurrencyId);
            return string.Format("{0}{1}", output, CommonFunctions.ComputeMd5(output.Replace("|", string.Empty) + key));
        }

        private static string DoBet(InputBase input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var key = CheckSign(client.PartnerId, string.Format("{0}{1}{2}{3}{4}", input.Func, input.Token, input.RandomNumber, input.Amount, client.CurrencyId), input.DataSignature);
                    var product = CacheManager.GetProductById(clientSession.ProductId);

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var document = documentBl.GetDocumentByExternalId(input.BetTransactionId, clientSession.Id, ProviderId,
                                                                            partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.Token,
                            ExternalProductId = product.ExternalId,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
                            TransactionId = input.BetTransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
						clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    var balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
                    var output = string.Format("OK|{0}|{1}|{2}|",input.RandomNumber, balance, client.CurrencyId);
                    return string.Format("{0}{1}", output, CommonFunctions.ComputeMd5(output.Replace("|", string.Empty) + key));
                }
            }
        }

        private static string DoWin(InputBase input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var key = CheckSign(client.PartnerId, string.Format("{0}{1}{2}{3}{4}", input.Func, input.Token, input.RandomNumber, input.Amount, client.CurrencyId), input.DataSignature);
                    var product = CacheManager.GetProductById(clientSession.ProductId);

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var betDocument = documentBl.GetDocumentByExternalId(input.BetTransactionId, clientSession.Id, ProviderId,
                                                                            partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument != null)
                    {
                        var amount = Convert.ToDecimal(input.Amount);
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        var document = documentBl.GetDocumentByExternalId(input.TransactionIdNs, clientSession.Id, ProviderId,
                                                                                partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (document == null)
                        {
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                State = state,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.Token,
                                GameProviderId = ProviderId,
                                ExternalProductId = product.ExternalId,
                                ProductId = clientSession.ProductId,
                                TransactionId = input.TransactionIdNs,
                                CreditTransactionId = betDocument.Id,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
                            });
							clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDocument, documentBl);
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
                    var balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance));
                    var output = string.Format("OK|{0}|{1}|{2}|", input.RandomNumber, balance, client.CurrencyId);
                    return string.Format("{0}{1}", output, CommonFunctions.ComputeMd5(output.Replace("|", string.Empty) + key));
                }
            }
        }

        private static string GameSessionEnd(InputBase input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null, false);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    string message = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", input.Func, input.Token, input.TransferIn,
                                                                                  input.TransferOut, input.BetsSum, input.BetsCount,
                                                                                  input.WinsSum, input.WinsCount, input.Currency);

                    var key = CheckSign(client.PartnerId, message, input.DataSignature);

                    var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.Token, ProviderId,
                        clientSession.Id, (int)BetDocumentStates.Uncalculated);

                    if (betDocuments != null)
                    {
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            State = (int)BetDocumentStates.Lost,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.Token,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        foreach (var betDocument in betDocuments)
                        {
                            listOfOperationsFromApi.TransactionId = string.Format("lost_" + betDocument.ExternalTransactionId);
                            listOfOperationsFromApi.CreditTransactionId = betDocument.Id;
                            clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDocument, documentBl);
                        }
                    }
                    var output = string.Format("OK|{0}|", input.Token);
                    return string.Format("{0}{1}", output, CommonFunctions.ComputeMd5(output.Replace("|", string.Empty) + key));
                }
            }
        }

        private static string CheckSign(int partnerId, string source, string inputSign)
        {
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.ZeusPlayDataSignatureKey);
            string hashSource = CommonFunctions.ComputeMd5(string.Format(source + secretKey));
            if (hashSource != inputSign)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            return secretKey;
        }
    }
}