﻿using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SunCity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class SunCityController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "116.93.57.10",
            "43.242.110.10",
            "159.117.141.10",
            "208.66.235.10",
            "115.85.146.14",
            "74.114.48.10",
            "118.163.83.109",
            "211.23.217.199"
        };

        private static Dictionary<string, string> Currencies { get; set; } = new Dictionary<string, string>
        {
            { "CNY", "rmb" },
            { "IDR", "idr_1000" }
        };
        private static readonly string WalletCode = "LiveDealerWallet";

        private static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SunCity).Id;
        private static readonly string AuthToken = "YmV0ZGVhbGlkcjEwMDA6c0Faa0xobmthY1dWc2FvZ3pXdjNiRnB5aERXTHZ1OWtqUXNiaGg0dU1DSw";
        [HttpPost]
        [Route("{partnerId}/api/SunCity/wallet/token")]
        public ActionResult Authorization(int partnerId, AuthInput authInput)
        {
            var jsonResponse = string.Empty;
            var authOutput = new AuthOutput
            {
                TokenType = "bearer",
                ExpiresIn = 3600,
                Scope = "wallet"
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var forCurrency = string.Empty;
                foreach (var curr in Currencies)
                {
                    if (authInput.client_id.ToLower().Contains(curr.Key.ToLower()) || authInput.client_id.ToLower().Contains(curr.Value.ToLower()))
                    {
                        forCurrency = curr.Key;
                        break;
                    }
                }
                var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SunCityOperatorID + forCurrency);
                var securKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SunCitySecureKey + forCurrency);
                if (operatorId != authInput.client_id && securKey != authInput.client_secret)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                authOutput.AccessToken = Convert.ToBase64String(Encoding.Default.GetBytes(AuthToken));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    authOutput.ErrorCode = fex.Detail.Id;
                    authOutput.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    authOutput.ErrorCode = Constants.Errors.GeneralException;
                    authOutput.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                authOutput.ErrorCode = Constants.Errors.GeneralException;
                authOutput.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(authOutput);
            Program.DbLogger.Info("Output=" + jsonResponse);
            return Ok(new StringContent(jsonResponse, Encoding.UTF8));
        }

        [HttpPost]
        [Route("{partnerId}/api/SunCity/wallet/balance")]
        public ActionResult GetBalance(int partnerId, BaseInput input)
        {
            var response = new BaseOutput();        
            var jsonResponse = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info("Input= " + JsonConvert.SerializeObject(input));
                var bodyStream = new StreamReader(Request.Body);
                var inputss = bodyStream.ReadToEnd(); ;
                Program.DbLogger.Info("InputString= " + inputss);

                //BaseBll.CheckIp(WhitelistedIps);
                CheckAuthorizationToken(partnerId);
                foreach (var us in input.Users)
                {
                    int clientId = 0;
                    Int32.TryParse(us.UserId, out clientId);
                    var client = CacheManager.GetClientById(clientId);
                    OutputUser outputUser;
                    if (client != null)
                    {
                        outputUser = new OutputUser
                        {
                            UserId = us.UserId,
                            Wallets = new List<Wallet>
                            {
                                new Wallet
                                {
                                    WalletCode = WalletCode,
                                    Currency = Currencies[client.CurrencyId],
                                    Name = string.Format("{0} {1}", client.FirstName, client.LastName),
                                    Balance=  BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance
                                }
                            }
                        };
                    }
                    else
                    {
                        outputUser = new OutputUser
                        {
                            ErrorCode = Constants.Errors.ClientNotFound
                        };
                    }
                    response.Users.Add(outputUser);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.ErrorCode = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ErrorCode = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.ErrorCode = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response);
            Program.DbLogger.Info("Output=" + jsonResponse);
            return Ok(new StringContent(jsonResponse, Encoding.UTF8));
        }

        [HttpPost]
        [Route("{partnerId}/api/SunCity/wallet/debit")]
        public ActionResult DoBet(int partnerId, BetInput input)
        {
            var response = new BetOutput();
            var jsonResponse = string.Empty;
            try
            {
                Program.DbLogger.Info("Input= " + JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                CheckAuthorizationToken(partnerId);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        foreach (var transaction in input.Transactions)
                        {
                            var transactionOutput = new TransactionOutput();
                            try
                            {
                                var product = CacheManager.GetProductByExternalId(ProviderId, transaction.gamecode);
                                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                                    product.Id);
                                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                                int clientId = 0;
                                Int32.TryParse(transaction.UserId, out clientId);
                                var client = CacheManager.GetClientById(clientId);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                var document = documentBl.GetDocumentByExternalId(transaction.ptxid, client.Id,
                                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                                var isDuplicateBet = true;
                                if (document == null)
                                {
                                    var operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        RoundId = transaction.roundid,
                                        GameProviderId = ProviderId,
                                        ExternalProductId = transaction.gamecode,
                                        ProductId = product.Id,
                                        TransactionId = transaction.ptxid,
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = transaction.Amount

                                    });
                                    isDuplicateBet = false;
                                    document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                    BaseHelpers.BroadcastBalance(client.Id);
                                }
                                transactionOutput.txid = document.Id.ToString();
                                transactionOutput.ptxid = transaction.ptxid;
                                transactionOutput.bal = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
                                transactionOutput.cur = Currencies[client.CurrencyId];
                                transactionOutput.dup = isDuplicateBet;
                            }
                            catch (FaultException<BllFnErrorType> fex)
                            {
                                if (fex.Detail != null)
                                {
                                    transactionOutput.ErrorCode = fex.Detail.Id;
                                    transactionOutput.ErrorDescription = fex.Detail.Message;
                                }
                                else
                                {
                                    transactionOutput.ErrorCode = Constants.Errors.GeneralException;
                                    transactionOutput.ErrorDescription = fex.Message;
                                }
                            }
                            catch (Exception ex)
                            {
                                transactionOutput.ErrorCode = Constants.Errors.GeneralException;
                                transactionOutput.ErrorDescription = ex.Message;
                            }
                            response.Transactions.Add(transactionOutput);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.ErrorCode = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ErrorCode = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.ErrorCode = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response);
            Program.DbLogger.Info("Output=" + jsonResponse);
            return Ok(new StringContent(jsonResponse, Encoding.UTF8));
        }

        [HttpPost]
        [Route("{partnerId}/api/SunCity/wallet/credit")]
        public ActionResult DoWin(int partnerId, BetInput input)
        {
            var response = new BetOutput();
            var jsonResponse = string.Empty;
            try
            {
                Program.DbLogger.Info("Input= " + JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                CheckAuthorizationToken(partnerId);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        foreach (var transaction in input.Transactions)
                        {
                            var transactionOutput = new TransactionOutput();
                            try
                            {
                                var product = CacheManager.GetProductByExternalId(ProviderId, transaction.gamecode);
                                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                                    product.Id);
                                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                                int clientId = 0;
                                Int32.TryParse(transaction.UserId, out clientId);
                                var client = CacheManager.GetClientById(clientId);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                var isDuplicateBet = true;
                                var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, transaction.roundid, ProviderId, client.Id);
                                if (betDocument == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                                var winDocument = documentBl.GetDocumentByExternalId(transaction.ptxid, client.Id, ProviderId,
                                    partnerProductSetting.Id, (int)OperationTypes.Win);

                                if (winDocument == null)
                                {
                                    isDuplicateBet = false;
                                    var state = (transaction.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                                    betDocument.State = state;
                                    var operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        RoundId = transaction.roundid,
                                        GameProviderId = ProviderId,
                                        OperationTypeId = (int)OperationTypes.Win,
                                        ExternalOperationId = null,
                                        ExternalProductId = transaction.gamecode,
                                        ProductId = betDocument.ProductId,
                                        TransactionId = transaction.ptxid,
                                        CreditTransactionId = betDocument.Id,
                                        State = state,
                                        Info = string.Empty,
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = transaction.Amount
                                    });

                                    winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                    BaseHelpers.BroadcastBalance(client.Id);
                                    transactionOutput.txid = winDocument.Id.ToString();
                                    transactionOutput.ptxid = transaction.ptxid;
                                    transactionOutput.bal = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
                                    transactionOutput.cur = Currencies[client.CurrencyId];
                                    transactionOutput.dup = isDuplicateBet;
                                }
                            }
                            catch (FaultException<BllFnErrorType> fex)
                            {
                                if (fex.Detail != null)
                                {
                                    transactionOutput.ErrorCode = fex.Detail.Id;
                                    transactionOutput.ErrorDescription = fex.Detail.Message;
                                }
                                else
                                {
                                    transactionOutput.ErrorCode = Constants.Errors.GeneralException;
                                    transactionOutput.ErrorDescription = fex.Message;
                                }
                            }
                            catch (Exception ex)
                            {
                                transactionOutput.ErrorCode = Constants.Errors.GeneralException;
                                transactionOutput.ErrorDescription = ex.Message;
                            }
                            response.Transactions.Add(transactionOutput);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.ErrorCode = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ErrorCode = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.ErrorCode = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response);
            Program.DbLogger.Info("Output=" + jsonResponse);
            return Ok(new StringContent(jsonResponse, Encoding.UTF8));
        }

        [HttpPost]
        [Route("{partnerId}/api/SunCity/wallet/cancel")]
        public ActionResult Rollback(int partnerId, BetInput input)
        {
            var response = new BetOutput();
            var jsonResponse = string.Empty;
            try
            {
                Program.DbLogger.Info("Input= " + JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                CheckAuthorizationToken(partnerId);
				using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
				{
					foreach (var transaction in input.Transactions)
					{
						var transactionOutput = new TransactionOutput();
						try
						{
							var product = CacheManager.GetProductByExternalId(ProviderId, transaction.gamecode);
							var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
								product.Id);
							if (partnerProductSetting == null || partnerProductSetting.Id == 0)
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

							int clientId = 0;
							Int32.TryParse(transaction.UserId, out clientId);
							var client = CacheManager.GetClientById(clientId);
							if (client == null)
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
							var isDuplicateBet = true;
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								GameProviderId = ProviderId,
								TransactionId = transaction.refptxid,
								ProductId = product.Id
							};
							var betDocument =
								documentBl.GetDocumentByExternalId(transaction.refptxid, client.Id, ProviderId, partnerProductSetting.Id,
								(int)OperationTypes.Bet);
							if (betDocument == null)
							{
								Program.DbLogger.Info(JsonConvert.SerializeObject(input));
								throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
							}
							if (betDocument.State != (int)BetDocumentStates.Deleted)
							{
								isDuplicateBet = false;
								betDocument = documentBl.RollbackProductTransactions(operationsFromProduct)[0];
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                            }
							transactionOutput.txid = betDocument.Id.ToString();
							transactionOutput.ptxid = transaction.ptxid;
							transactionOutput.bal = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
							transactionOutput.cur = client.CurrencyId;
							transactionOutput.dup = isDuplicateBet;
						}
						catch (FaultException<BllFnErrorType> fex)
						{
							if (fex.Detail != null)
							{
								transactionOutput.ErrorCode = fex.Detail.Id;
								transactionOutput.ErrorDescription = fex.Detail.Message;
							}
							else
							{
								transactionOutput.ErrorCode = Constants.Errors.GeneralException;
								transactionOutput.ErrorDescription = fex.Message;
							}
						}
						catch (Exception ex)
						{
							transactionOutput.ErrorCode = Constants.Errors.GeneralException;
							transactionOutput.ErrorDescription = ex.Message;
						}
						response.Transactions.Add(transactionOutput);
					}
				}
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail != null)
                {
                    response.ErrorCode = fex.Detail.Id;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ErrorCode = Constants.Errors.GeneralException;
                    response.ErrorDescription = fex.Message;
                }
            }
            catch (Exception ex)
            {
                response.ErrorCode = Constants.Errors.GeneralException;
                response.ErrorDescription = ex.Message;
            }
            jsonResponse = JsonConvert.SerializeObject(response);
            Program.DbLogger.Info("Output=" + jsonResponse);
            return Ok(new StringContent(jsonResponse, Encoding.UTF8));
        }

        private void CheckAuthorizationToken(int partnerId)
        {
            var auth = Request.Headers["Authorization"].ToString();
            auth = auth.Replace("Bearer", string.Empty).Replace("bearer", string.Empty).Trim();
            var partner = CacheManager.GetPartnerById(partnerId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SunCityOperatorID + Currencies[partner.CurrencyId]);
            var securKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SunCitySecureKey + Currencies[partner.CurrencyId]);

            if(Convert.ToBase64String(Encoding.Default.GetBytes(AuthToken)).ToLower()!= auth.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }
    }
}
