using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.Betsy;
using IqSoft.CP.BLL.Services;
using System;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using Microsoft.Extensions.Primitives;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class BetsyController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Betsy).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "52.58.175.198",
            "18.198.240.104",
            "52.29.99.138",
            "3.64.21.163",
            "18.158.251.250",
            "18.198.90.137",
            "3.126.72.114",
            "18.196.140.152",
            "18.198.240.104",
            "3.126.142.213",
            "52.28.194.175",
            "3.127.50.84",
            "18.157.100.238",
            "18.157.231.148",
            "80.92.236.9",
            "35.156.18.105",
            "18.159.27.166",
            "54.93.162.147",
            "35.158.22.146",
            "18.157.209.63",
            "3.65.70.96"
        };

        private static string GenerateJWTToken(int partnerId, string request)
        {
            var key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BetsyApiKey);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var payload = JwtPayload.Deserialize(request);
            if (payload.ContainsKey("metadata"))
            {
                payload["metadata"] = JsonConvert.DeserializeObject<BetInput>(request, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }).Metadatas;
            }
            var headers = new JwtHeader(signingCredentials);
            var secToken = new JwtSecurityToken(headers, payload);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }

        private void CheckSignature(int partnerId, string request)
        {
            if (!Request.Headers.ContainsKey("x-sign-jws"))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            var jws = Request.Headers["x-sign-jws"];
            if (string.IsNullOrEmpty(jws))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            var token = GenerateJWTToken(partnerId, request);
            if (jws.ToString().Split('.')[2] != token.Split('.')[2])
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        [HttpPost]
        [Route("{partnerId}/api/Betsy/user/profile")]
        public ActionResult Authentication([FromRoute] int partnerId)
        {
            var inputString = string.Empty;
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var input = JsonConvert.DeserializeObject<BaseInput>(inputString);
                if (string.IsNullOrEmpty(input.Token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongToken);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                partnerId = client.PartnerId;
                CheckSignature(client.PartnerId, inputString);

                var response = new
                {
                    userId = client.Id.ToString(),
                    currency = client.CurrencyId,
                    currencies = new List<string> { client.CurrencyId },
                    isTest = false, // change
                    customFields = new
                    {
                        firstName = client.FirstName,
                        lastname = client.LastName,
                        isVerified = true, // check with email or mobile or kyc
                        userCountry = clientSession.Country,
                        userSessionIp = clientSession.LoginIp,
                    }
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(client.PartnerId, JsonConvert.SerializeObject(response)));
                Program.DbLogger.Info("InputData: " + inputString +"  Output: " + JsonConvert.SerializeObject(response));

                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                //if (fex.Detail.Id == Constants.Errors.WrongHash ||fex.Detail.Id == Constants.Errors.WrongToken)
                //    return Unauthorized(errorOutput);
                return Unauthorized(errorOutput);
            }
            catch (Exception ex)
            {
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    message = ex.Message
                };
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                return Ok(errorOutput);
            }
        }


        [HttpPost]
        [Route("{partnerId}/api/Betsy/user/balance")]
        public ActionResult GetBalance([FromRoute] int partnerId)
        {
            var inputString = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }

                var input = JsonConvert.DeserializeObject<BalanceInput>(inputString);
                if (string.IsNullOrEmpty(input.Token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongToken);

                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                partnerId = client.PartnerId;
                CheckSignature(client.PartnerId, inputString);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (client.Id.ToString() != input.UserId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var response = new
                {
                    userId = client.Id.ToString(),
                    currency = client.CurrencyId,
                    amount = Convert.ToDecimal(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance))
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(client.PartnerId, JsonConvert.SerializeObject(response)));
                Program.DbLogger.Info("InputData: " + inputString +"  Output: " + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                //if (fex.Detail.Id == Constants.Errors.WrongHash || fex.Detail.Id == Constants.Errors.WrongToken)
                //    return Unauthorized(errorOutput);
                return Unauthorized(errorOutput);
            }
            catch (Exception ex)
            {
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    message = ex.Message
                };
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                return Ok(errorOutput);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/Betsy/payment/check")]
        public ActionResult CheckPayment([FromRoute] int partnerId)
        {
            var inputString = string.Empty;
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var input = JsonConvert.DeserializeObject<CheckInput>(inputString);
                if (!int.TryParse(input.UserId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientId);
                partnerId = client.PartnerId;
                CheckSignature(client.PartnerId, inputString);

                using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
                var document = documentBl.GetDocumentOnlyByExternalId("Bet_" + input.TransactionId, ProviderId, client.Id, (int)OperationTypes.BetRollback);
                if (document==null)
                    document = documentBl.GetDocumentOnlyByExternalId("Win_" + input.TransactionId, ProviderId, client.Id, (int)OperationTypes.Win);
                if (document == null)
                    document = documentBl.GetDocumentOnlyByExternalId("Bet_" + input.TransactionId, ProviderId, client.Id, (int)OperationTypes.Bet);
                if (document==null)
                    document = documentBl.GetDocumentOnlyByExternalId("RollbackedWin_" + input.TransactionId, ProviderId, client.Id, (int)OperationTypes.Win);
                if (document==null)
                    document = documentBl.GetDocumentOnlyByExternalId("Win_" + input.TransactionId, ProviderId, client.Id, (int)OperationTypes.WinRollback);
                if (document==null)
                    document = documentBl.GetDocumentOnlyByExternalId("CashoutWin_" + input.TransactionId, ProviderId, client.Id, (int)OperationTypes.Win);
                if (document==null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);

                var type = document.ExternalTransactionId.Contains("Rollbacked") || document.OperationTypeId ==  (int)OperationTypes.BetRollback ||
                                                                                    document.OperationTypeId ==  (int)OperationTypes.WinRollback ?
                           3 : (document.ExternalTransactionId.Contains("Bet_") ? 1 : 2);
                var response = new
                {
                    transactionId = input.TransactionId,
                    currency = client.CurrencyId,
                    amount = document.ExternalTransactionId.Contains("Rollbacked") ||
                             document.OperationTypeId ==  (int)OperationTypes.WinRollback ? 0 : document.Amount,
                    transactionTime = document.LastUpdateTime.ToString(),
                    type
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(client.PartnerId, JsonConvert.SerializeObject(response)));
                Program.DbLogger.Info("InputData: " + inputString +"  Output: " + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                return Unauthorized(errorOutput);
            }
            catch (Exception ex)
            {
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    message = ex.Message
                };
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                return Ok(errorOutput);
            }
        }

        [HttpPost]
        [HttpPut]
        [Route("{partnerId}/api/Betsy/payment/bet")]
        public ActionResult DoBetWin([FromRoute] int partnerId)
        {
            var inputString = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                var input = JsonConvert.DeserializeObject<BetInput>(inputString);
                if (string.IsNullOrEmpty(input.Token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongToken);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null, input.Type == (int)BetsyHelpers.TransactionTypes.Bet);
                var client = CacheManager.GetClientById(clientSession.Id);
                partnerId = client.PartnerId;
                if (input.Type != (int)BetsyHelpers.TransactionTypes.Bet) // ????
                    CheckSignature(client.PartnerId, inputString);
                else
                {
                    if (!Request.Headers.ContainsKey("x-sign-jws"))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var jws = Request.Headers["x-sign-jws"];
                    if (string.IsNullOrEmpty(jws))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                }
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (client.Id.ToString() != input.UserId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (!input.Amount.HasValue)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);

                var transactionDate = DateTime.UtcNow.ToString();
                if (input.Type == (int)BetsyHelpers.TransactionTypes.Bet)
                {
                    if (input.ErrorCode.HasValue && input.ErrorCode.Value == 50)
                        transactionDate = Rollback(input, clientSession, client);
                    else
                        transactionDate = DoBet(input, clientSession, client);
                }
                else if (input.Type == (int)BetsyHelpers.TransactionTypes.Settle && !string.IsNullOrEmpty(input.ResultType))
                {
                    if (input.ResultType.ToLower() == "won" || input.ResultType.ToLower() == "refund" || input.ResultType.ToLower() == "lost")
                        transactionDate = DoWin(input, clientSession, client);
                    else if (input.ResultType.ToLower() == "cashout")
                    {
                        transactionDate = DoCashout(input, clientSession, client);
                    }
                }
                else if (input.Type == (int)BetsyHelpers.TransactionTypes.Rollback)
                    transactionDate = Rollback(input, clientSession, client);
                var response = new
                {
                    transactionId = input.TransactionId,
                    transactionTime = transactionDate
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(client.PartnerId, JsonConvert.SerializeObject(response)));
                Program.DbLogger.Info("InputData: " + inputString +"  Output: " + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("InputData: " + inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message +"____Token: " + Request.Headers["x-sign-jws"]);
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    message = fex.Detail.Message
                };
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                return Unauthorized(errorOutput);
            }
            catch (Exception ex)
            {
                var errorOutput = new
                {
                    code = BetsyHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    message = ex.Message
                };
                Program.DbLogger.Error(inputString + "_" + ex.Message);
                Program.DbLogger.Error("InputData: " + inputString + "_" + ex.Message +"____Token: " + Request.Headers["x-sign-jws"]);
                Response.Headers.Add("x-sign-jws", GenerateJWTToken(partnerId, JsonConvert.SerializeObject(errorOutput)));
                return Ok(errorOutput);
            }
        }

        private static string DoBet(BetInput input, SessionIdentity clientSession, BllClient client)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            if (product.GameProviderId != ProviderId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            var transactionId = string.Format("Bet_{0}", input.TransactionId);
            var betDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                 partnerProductSetting.Id, (int)OperationTypes.Bet);
            if (betDocument != null)
            {
                if (input.Metadatas == null)
                    RollbackDocument(string.Format("Win_{0}", input.TransactionId), clientSession, documentBl, input.RequestId);
                else
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);
            }
            else if (betDocument == null)
            {
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = clientSession.SessionId,
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
                    ProductId = product.Id,
                    TransactionId = transactionId,
                    OperationItems = new List<OperationItemFromProduct>()
                };
                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                {
                    Client = client,
                    Amount = input.Amount.Value,
                    DeviceTypeId = clientSession.DeviceType
                });
                betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
            }
            return betDocument.CreationTime.ToString();
        }

        private static string DoWin(BetInput input, SessionIdentity clientSession, BllClient client)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            if (product.GameProviderId != ProviderId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            var betDocument = documentBl.GetDocumentByExternalId(string.Format("Bet_{0}", input.TransactionId), client.Id, ProviderId,
                                                                  partnerProductSetting.Id, (int)OperationTypes.Bet);
            if (betDocument == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
            if (betDocument.State == (int)BetDocumentStates.Deleted)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyRollbacked);
            var transactionId = string.Format("Win_{0}", input.TransactionId);
            var transactionTime = DateTime.UtcNow;
            var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                 partnerProductSetting.Id, (int)OperationTypes.Win);
            if (winDocument != null)
            {
                transactionTime = winDocument.CreationTime;
                RollbackDocument(transactionId, clientSession, documentBl, input.RequestId);
            }
            var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
            betDocument.State = state;
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                CurrencyId = client.CurrencyId,
                RoundId = input.TransactionId,
                GameProviderId = ProviderId,
                OperationTypeId = (int)OperationTypes.Win,
                ExternalOperationId = null,
                ExternalProductId = product.ExternalId,
                ProductId = betDocument.ProductId,
                TransactionId = transactionId,
                CreditTransactionId = betDocument.Id,
                State = state,
                Info = string.Empty,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = input.Amount.Value,
                DeviceTypeId = clientSession.DeviceType
            });

            var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            BaseHelpers.BroadcastWin(new ApiWin
            {
                GameName = product.NickName,
                ClientId = client.Id,
                ClientName = client.FirstName,
                Amount = input.Amount.Value,
                CurrencyId = client.CurrencyId,
                PartnerId = client.PartnerId,
                ProductId = product.Id,
                ProductName = product.NickName,
                ImageUrl = product.WebImageUrl
            });
            if (winDocument!= null)
                return transactionTime.ToString();
            return doc[0].CreationTime.ToString();
        }
        private static string DoCashout(BetInput input, SessionIdentity clientSession, BllClient client)
        {
            var product = CacheManager.GetProductById(clientSession.ProductId);
            if (product.GameProviderId != ProviderId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var winDonument = documentBl.GetDocumentByExternalId("CashoutWin_" + input.TransactionId, client.Id, ProviderId,
                                                               partnerProductSetting.Id, (int)OperationTypes.Win);
            if (winDonument == null)
            {
                var betOperationsFromProduct = new ListOfOperationsFromApi
                {
                    CurrencyId = client.CurrencyId,
                    GameProviderId = ProviderId,
                    ProductId = product.Id,
                    RoundId = input.TransactionId,
                    State = (int)BetDocumentStates.Won,
                    TransactionId = "CashoutWin_" + input.TransactionId,
                    OperationItems = new List<OperationItemFromProduct>()
                };
                betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                {
                    Client = client,
                    Amount = 0m
                });
                var betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl);

                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = betDocument.SessionId,
                    CurrencyId = client.CurrencyId,
                    RoundId = input.TransactionId,
                    GameProviderId = ProviderId,
                    OperationTypeId = (int)OperationTypes.Win,
                    ExternalOperationId = null,
                    ExternalProductId = product.ExternalId,
                    ProductId = betDocument.ProductId,
                    TransactionId = "CashoutWin_" + input.TransactionId,
                    CreditTransactionId = betDocument.Id,
                    State = (int)BetDocumentStates.Won,
                    OperationItems = new List<OperationItemFromProduct>()
                };
                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                {
                    Client = client,
                    Amount = Convert.ToDecimal(input.Amount),
                    DeviceTypeId = betDocument.DeviceTypeId
                });
                var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastWin(new ApiWin
                {
                    GameName = product.NickName,
                    ClientId = client.Id,
                    ClientName = client.FirstName,
                    Amount = Convert.ToDecimal(input.Amount),
                    CurrencyId = client.CurrencyId,
                    PartnerId = client.PartnerId,
                    ProductId = product.Id,
                    ProductName = product.NickName,
                    ImageUrl = product.WebImageUrl
                });
                return doc[0].CreationTime.ToString();
            }
            return winDonument.CreationTime.ToString();
        }
        private static string Rollback(BetInput input, SessionIdentity clientSession, BllClient client)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            var transactionId = string.Format("Win_{0}", input.TransactionId);
            var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                 partnerProductSetting.Id, (int)OperationTypes.Win);
            if (winDocument != null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                GameProviderId = ProviderId,
                TransactionId = "Bet_" + input.TransactionId,
                ExternalProductId = product.ExternalId,
                ProductId = product.Id
            };
            var documents = documentBl.RollbackProductTransactions(operationsFromProduct, true);
            if (documents == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
            BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
            return documents[0].CreationTime.ToString();
        }

        private static void RollbackDocument(string transactionId, SessionIdentity clientSession, DocumentBll documentBl, string requestId)
        {
            var product = CacheManager.GetProductById(clientSession.ProductId);
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                GameProviderId = ProviderId,
                TransactionId = transactionId,
                ExternalProductId = product.ExternalId,
                ProductId = product.Id
            };
            documentBl.RollbackProductTransactions(operationsFromProduct, true, string.Format("RollbackedWin_{0}_{1}", transactionId, requestId));
            BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
        }
    }
}