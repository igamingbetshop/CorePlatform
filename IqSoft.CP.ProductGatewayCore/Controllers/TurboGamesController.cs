using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using IqSoft.CP.ProductGateway.Models.TurboGames;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.DAL;
using Microsoft.Extensions.Primitives;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class TurboGamesController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TurboGames).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "18.198.240.104",//STAGE
            "3.127.172.53",//STAGE
            "35.156.18.105",//PROD
            "18.159.27.166",//PROD
            "54.93.162.147",//PROD
            "46.118.151.123", //QA
        };
        private enum TransactionTypes
        {
            Bet = 1,
            Win = 2,
            Rollback = 3
        }

        [HttpPost]
        [Route("{partnerId}/api/TurboGames/user/profile")]
        public ActionResult Authenticate(BaseInput input)
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
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                CheckSignature(client.PartnerId, inputString);
                return Ok(new
                {
                    userId = client.Id.ToString(),
                    currency = client.CurrencyId,
                    currencies = new List<string> { client.CurrencyId },
                    isTest = true,
                    customFields = new
                    {
                        username = client.UserName
                    }
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                return BadRequest(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                return BadRequest(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/TurboGames/user/balance")]
        public ActionResult GetBalance(BalanceInput input)
        {
            var inputString = string.Empty;
            object response;
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
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                CheckSignature(client.PartnerId, inputString);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response = new
                {
                    userId = client.Id.ToString(),
                    currency = client.CurrencyId,
                    amount = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2)
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                response = new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                };
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                response = new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                };
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/TurboGames/payment/check")]
        public ActionResult CheckTransaction(TransactionInput input)
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
                if (!int.TryParse(input.ClientId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                CheckSignature(client.PartnerId, inputString);
                using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
                var betDocument = documentBl.GetDocumentOnlyByExternalId(input.TransactionId, ProviderId, client.Id, (int)OperationTypes.Bet);
                if (betDocument == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongDocumentId);
                return Ok( new
                {
                    transactionId = betDocument.Id.ToString(),
                    currency = client.CurrencyId,
                    amount = betDocument.Amount,
                    type = (int)TransactionTypes.Bet,
                    transactionTime = betDocument.CreationTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                return BadRequest(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                return BadRequest(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
            }
        }

        [HttpPost, HttpPut]
        [Route("{partnerId}/api/TurboGames/payment/bet")]
        public ActionResult ProcessTransaction(TransactionInput input)
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
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: input.Type == 1);
                var client = CacheManager.GetClientById(clientSession.Id);
                CheckSignature(client.PartnerId, inputString);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (client.Id.ToString() != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (!input.Amount.HasValue)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                Document transactionDocument;
                switch (input.Type)
                {
                    case (int)TransactionTypes.Bet:
                        transactionDocument = DoBet(input.TransactionId, input.Amount.Value, client, clientSession);
                        break;
                    case (int)TransactionTypes.Win:
                        transactionDocument = DoWin(input.RequestId, input.TransactionId, input.Amount.Value, client, clientSession);
                        break;
                    case (int)TransactionTypes.Rollback:
                        transactionDocument = Rollback(input.TransactionId, clientSession);
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                }
                return Ok(new
                {
                    transactionId = string.Format("transaction_{0}", transactionDocument?.Id.ToString()),
                    transactionTime = transactionDocument != null ? transactionDocument.CreationTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK") : DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + fex.Detail.Message);
                return BadRequest(new
                {
                    code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id,
                    message = fex.Detail.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(inputString + "_   ErrorMessage: " + ex.Message);
                return BadRequest(new
                {
                    code = Constants.Errors.GeneralException,
                    message = ex.Message
                });
            }
        }

        private static Document DoBet(string transactionId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            var betDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                 partnerProductSetting.Id, (int)OperationTypes.Bet);
            if (betDocument != null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                CurrencyId = client.CurrencyId,
                GameProviderId = ProviderId,
                ExternalProductId = product.ExternalId,
                ProductId = clientSession.ProductId,
                RoundId = transactionId,
                TransactionId = transactionId,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = amount,
                DeviceTypeId = clientSession.DeviceType
            });
            betDocument =  clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
            BaseHelpers.BroadcastBalance(client.Id); BaseHelpers.BroadcastBalance(client.Id);
            BaseHelpers.BroadcastBalance(client.Id);
            return betDocument;
        }

        private static Document DoWin(string transactionId, string betTransactionId, decimal amount, BllClient client, SessionIdentity clientSession)
        {
            using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
            using var documentBl = new DocumentBll(clientBl);
            var product = CacheManager.GetProductById(clientSession.ProductId);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            var betDocument = documentBl.GetDocumentByExternalId(betTransactionId, client.Id, ProviderId,
                                                                 partnerProductSetting.Id, (int)OperationTypes.Bet);
            if (betDocument == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);

            var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
            if (winDocument != null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

            var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
            betDocument.State = state;
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                CurrencyId = client.CurrencyId,
                RoundId = transactionId,
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
                Amount = amount
            });

            winDocument =  clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
            BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
            BaseHelpers.BroadcastWin(new ApiWin
            {
                GameName = product.NickName,
                ClientId = client.Id,
                ClientName = client.FirstName,
                Amount = amount,
                CurrencyId = client.CurrencyId,
                PartnerId = client.PartnerId,
                ProductId = product.Id,
                ProductName = product.NickName,
                ImageUrl = product.WebImageUrl
            });
            return winDocument;
        }

        private static Document Rollback(string transactionId, SessionIdentity clientSession)
        {
            try
            {
                using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = transactionId,
                    ProductId = product.Id,
                    ExternalProductId = product.ExternalId
                };
                var rollbackDocumnent = documentBl.RollbackProductTransactions(operationsFromProduct)[0];
                BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                return rollbackDocumnent;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
                    throw;
                return null;
            }
        }

        private static string GenerateJWTToken(int partnerId, string request)
        {
            var key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TurboGamesApiKey);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var payload = JwtPayload.Deserialize(request);
            var headers = new JwtHeader(signingCredentials);
            if (headers.ContainsKey("cty"))
                headers.Remove("cty");
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
    }
}