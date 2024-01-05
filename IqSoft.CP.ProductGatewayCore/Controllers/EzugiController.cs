using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ServiceModel;
using IqSoft.CP.ProductGateway.Models.Ezugi;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using IqSoft.CP.Integration.Platforms.Helpers;
using System.Text;
using System.Security.Cryptography;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using IqSoft.CP.Common.Models.CacheModels;
using System.IO;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class EzugiController : ControllerBase
    {
        private readonly static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Ezugi).Id;
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "52.16.138.24",
            "52.16.33.81",
            "52.16.124.91",
            "145.239.222.15",
            "109.97.118.250",
            "52.211.45.101"
        };

        [HttpPost]
        [Route("{partnerId}/api/Ezugi/Authentication")]
        public AuthenticationOutput Authentication()
        {
            var inputString = string.Empty;
            var input = new AuthenticationInput();
            var response = new AuthenticationOutput();
            BllClient client;
            SessionIdentity clientSession;
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                input = JsonConvert.DeserializeObject<AuthenticationInput>(inputString);
                try
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                }
                catch
                {
                    Program.DbLogger.Info("NewIp: " + Request.Headers["CF-Connecting-IP"]);
                }
                clientSession = CheckSession(input.Token);
                client = CacheManager.GetClientById(clientSession.Id);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                CheckHash(client.PartnerId, inputString);
                using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var newSession = clientBl.RefreshClientSession(input.Token, true);
                BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                var currency = client.CurrencyId;
                if (EzugiHelpers.UnsuppordedCurrenies.Contains(client.CurrencyId))
                    currency = Constants.Currencies.USADollar;

                response.OperatorId = input.OperatorId;
                response.ClientId = client.Id.ToString();
                response.NickName = client.UserName;
                response.Token = newSession.Token;
                response.PlayerTokenAtLaunch = input.Token;
                response.Currency = currency;
                response.Language = newSession.LanguageId;
                response.ClientIp = newSession.Ip;
                response.ErrorCode = EzugiHelpers.ErrorCodes.Success;
                response.ErrorDescription = EzugiHelpers.GetError(EzugiHelpers.ErrorCodes.Success).Item2;
                response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                response.Timestamp = GetUnixTime();
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = EzugiHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response = new AuthenticationOutput
                {
                    OperatorId = input.OperatorId,
                    PlayerTokenAtLaunch = input.Token,
                    ErrorCode = error.Item1,
                    ErrorDescription = error.Item2
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response = new AuthenticationOutput()
                {
                    OperatorId = input.OperatorId,
                    PlayerTokenAtLaunch = input.Token,
                    ErrorCode = error.Item1,
                    ErrorDescription = error.Item2
                };
            }
            response.Timestamp = GetUnixTime();
            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("{partnerId}/api/Ezugi/Debit")]
        public async Task<IActionResult> Credit()
        {
            var inputString = string.Empty;
            var input = new DebitInput();
            var response = new DebitOutput();
            var clientSession = new SessionIdentity();
            var client = new BllClient();
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                input = JsonConvert.DeserializeObject<DebitInput>(inputString);
                client = CacheManager.GetClientById(input.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response.ClientId = input.ClientId.ToString();
                response.OperatorId = input.OperatorId;
                response.RoundId = input.RoundId;
                response.Token = input.Token;
                response.TransactionId = input.TransactionId;
                response.Currency = input.Currency;
                response.Timestamp = GetUnixTime();
                try
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                }
                catch
                {
                    Program.DbLogger.Info("NewIp: " + Request.Headers["CF-Connecting-IP"]);
                }
                clientSession = CheckSession(input.Token, input.ClientId);
                CheckHash(client.PartnerId, inputString);
                await Task.Run(() => ProcessCredit(input, clientSession, response));
                response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = EzugiHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound &&
                    error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound &&
                    fex.Detail.Id != Constants.Errors.WrongHash)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                }
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound && error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                }
            }
            response.Timestamp = GetUnixTime();
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Ezugi/Credit")]
        public async Task<IActionResult> Debit()
        {
            var inputString = string.Empty;
            var input = new CreditInput();
            var response = new CreditOutput();

            var clientSession = new SessionIdentity();
            var client = new BllClient();
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
                    Program.DbLogger.Info("NewIp: " + Request.Headers["CF-Connecting-IP"]);
                }
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                input = JsonConvert.DeserializeObject<CreditInput>(inputString);
                client = CacheManager.GetClientById(input.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response.OperatorId = input.OperatorId;
                response.RoundId = input.RoundId;
                response.ClientId = input.ClientId.ToString();
                response.Token = input.Token;
                response.TransactionId = input.TransactionId;
                response.Currency = input.Currency;
                response.Timestamp = GetUnixTime();
                clientSession = CheckSession(input.Token, input.ClientId, false);
                CheckHash(client.PartnerId, inputString);
                await Task.Run(() => ProcessDebit(input, clientSession, response));
                response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = EzugiHelpers.GetError(fex.Detail == null
                                ? Constants.Errors.GeneralException
                                : fex.Detail.Id);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound &&
                    error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound &&
                    fex.Detail.Id != Constants.Errors.WrongHash)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ParentId ?? 0);

                }
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + "_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound && error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                }
            }
            response.Timestamp = GetUnixTime();
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Ezugi/Rollback")]
        public BaseOutput Rollback()
        {
            var inputString = string.Empty;
            var response = new RollbackOutput();
            var input = new RollbackInput();
            var clientSession = new SessionIdentity();
            var client = new BllClient();
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    inputString = reader.ReadToEnd();
                }
                input = JsonConvert.DeserializeObject<RollbackInput>(inputString);
                client = CacheManager.GetClientById(input.ClientId);
                response.OperatorId = input.OperatorId;
                response.RoundId = input.RoundId;
                response.ClientId = input.ClientId.ToString();
                response.Token = input.Token;
                response.TransactionId = input.TransactionId;
                response.Currency = input.Currency;
                response.Timestamp = GetUnixTime();
                try
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                }
                catch
                {
                    Program.DbLogger.Info("NewIp: " + Request.Headers["CF-Connecting-IP"]);
                }
                clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                CheckHash(client.PartnerId, inputString);
                using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(documentBl);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.TableId.ToString());
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ProductNotAllowedForThisPartner);
                var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id,
                    ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                try
                {
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    if (EzugiHelpers.UnsuppordedCurrenies.Contains(client.CurrencyId))
                        input.RollbackAmount = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, input.RollbackAmount);
                    else if (Math.Abs(betDocument.Amount - Convert.ToDecimal(input.RollbackAmount)) > Constants.Delta)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.TransactionId,
                        ProductId = product.Id
                    };
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client, operationsFromProduct, doc[0]);
                        }
                        catch (Exception ex)
                        {
                            Program.DbLogger.Error(ex.Message);
                        }
                    }
                }
                catch (FaultException<BllFnErrorType>)
                {
                    CacheManager.SetFutureRollback(Constants.CacheItems.EzugiRollback, input.TransactionId, input.TransactionId);
                    throw;
                }
                response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
                response.ErrorCode = EzugiHelpers.ErrorCodes.Success;
                response.ErrorDescription = EzugiHelpers.GetError(EzugiHelpers.ErrorCodes.Success).Item2;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var error = EzugiHelpers.GetError(fex.Detail == null
                                ? Constants.Errors.GeneralException
                                : fex.Detail.Id);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound &&
                    error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound &&
                    fex.Detail.Id != Constants.Errors.WrongHash)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                }
                Program.DbLogger.Error(fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound && error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
                }
            }
            response.Timestamp = GetUnixTime();
            return response;
        }

        private static void ProcessCredit(DebitInput input, SessionIdentity clientSession, DebitOutput response)
        {
            using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(documentBl);
            var client = CacheManager.GetClientById(clientSession.Id);
            var product = CacheManager.GetProductByExternalId(ProviderId, input.TableId.ToString());
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                product.Id);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
            var document = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id,
                ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
            if (document != null)
                throw BaseBll.CreateException(String.Empty, Constants.Errors.TransactionAlreadyExists);
            response.ErrorCode = EzugiHelpers.ErrorCodes.Success;
            var currency = input.Currency;
            if (EzugiHelpers.UnsuppordedCurrenies.Contains(client.CurrencyId))
            {
                input.DebitAmount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, input.DebitAmount);
                currency = client.CurrencyId;
            }
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                CurrencyId = currency,
                RoundId = string.Format("{0}-{1}-{2}", input.RoundId, input.BetTypeID, input.SeatId),
                GameProviderId = ProviderId,
                ExternalProductId = input.TableId.ToString(),
                ProductId = product.Id,
                Info = string.Empty,
                TransactionId = input.TransactionId,
                OperationItems = new List<OperationItemFromProduct>()
            };
            var operationItem = new OperationItemFromProduct
            {
                Amount = input.DebitAmount,
                Client = CacheManager.GetClientById(clientSession.Id),
                DeviceTypeId = clientSession.DeviceType
            };
            operationsFromProduct.OperationItems.Add(operationItem);
            var doc = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
            var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.EzugiRollback, input.TransactionId);
            if (!string.IsNullOrEmpty(rollback))
            {
                var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = input.TransactionId,
                    ExternalProductId = input.TableId.ToString(),
                    ProductId = clientSession.ProductId
                };
                try
                {
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                }
                catch {; }
                response.ErrorCode = EzugiHelpers.ErrorCodes.GeneralError;
            }
            else
            {
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                {
                    try
                    {
                        ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, doc);
                    }
                    catch (Exception ex)
                    {
                        Program.DbLogger.Error(ex.Message);
                        documentBl.RollbackProductTransactions(operationsFromProduct);
                        throw;
                    }
                }
            }
            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            BaseHelpers.BroadcastBalance(client.Id);
            response.ClientId = client.Id.ToString();
            //   response.Balance = GetBalance(client, clientSession.ParentId ?? 0);                  
            response.ErrorDescription = EzugiHelpers.GetError(EzugiHelpers.ErrorCodes.Success).Item2;
            //  response.Timestamp = GetUnixTime();
        }

        private static void ProcessDebit(CreditInput input, SessionIdentity clientSession, CreditOutput response)
        {
            using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(documentBl);
            var client = CacheManager.GetClientById(clientSession.Id);
            string seatId = input.SeatId;
            if (input.BetTypeID == EzugiHelpers.BetTypeCodes.Split || input.BetTypeID == EzugiHelpers.BetTypeCodes.SplitBB)
            {
                seatId = SplitCreditSeatId(seatId);
            }

            string betRoundId = string.Format("{0}-{1}-{2}", input.RoundId, EzugiHelpers.BetTypes[input.BetTypeID], seatId);
            var product = CacheManager.GetProductByExternalId(ProviderId, input.TableId.ToString());
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                product.Id);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ProductNotAllowedForThisPartner);
            DAL.Document betDocument = null;
            betDocument = documentBl.GetDocumentByExternalId(input.DebitTransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
            if (betDocument == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
            var currency = input.Currency;
            if (EzugiHelpers.UnsuppordedCurrenies.Contains(client.CurrencyId))
            {
                input.CreditAmount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, input.CreditAmount);
                currency = client.CurrencyId;
            }
            var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

            if (winDocument != null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
            if (betDocument.State != (int)BetDocumentStates.Uncalculated)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

            int state = input.CreditAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
            betDocument.State = state;
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                SessionId = clientSession.SessionId,
                CurrencyId = currency,
                RoundId = string.Format("{0}-{1}-{2}", input.RoundId, input.BetTypeID, input.SeatId),
                OperationTypeId = (int)OperationTypes.Win,
                GameProviderId = ProviderId,
                ProductId = product.Id,
                ExternalProductId = input.TableId.ToString(),
                State = state,
                TransactionId = input.TransactionId,
                CreditTransactionId = betDocument.Id,
                Info = Enum.GetName(typeof(EzugiHelpers.ReturnReasonCodes), input.ReturnReason),
                OperationItems = new List<OperationItemFromProduct>()
            };
            var operationItem = new OperationItemFromProduct
            {
                Amount = input.CreditAmount,
                Client = client,
            };
            operationsFromProduct.OperationItems.Add(operationItem);
            var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
            if (isExternalPlatformClient)
            {
                try
                {
                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                          (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0]);
                }
                catch (FaultException<BllFnErrorType> ex)
                {
                    var message = ex.Detail == null
                        ? new ResponseBase
                        {
                            ResponseCode = Constants.Errors.GeneralException,
                            Description = ex.Message
                        }
                        : new ResponseBase
                        {
                            ResponseCode = ex.Detail.Id,
                            Description = ex.Detail.Message
                        };
                    Program.DbLogger.Error(JsonConvert.SerializeObject(message));
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    throw;
                }
                catch (Exception ex)
                {
                    Program.DbLogger.Error(ex.Message);
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    throw;
                }
            }
            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            BaseHelpers.BroadcastWin(new ApiWin
            {
                GameName = partnerProductSetting.NickName,
                ClientName = client.FirstName,
                Amount = input.CreditAmount,
                CurrencyId = client.CurrencyId,
                PartnerId = client.PartnerId,
                ProductId = product.Id,
                ProductName = product.NickName,
                ImageUrl = product.WebImageUrl
            });

            // response.Balance = GetBalance(client, clientSession.ParentId ?? 0);
            response.ErrorCode = EzugiHelpers.ErrorCodes.Success;
            response.ErrorDescription = (input.CreditAmount <= 0) ? "Ok" : EzugiHelpers.GetError(EzugiHelpers.ErrorCodes.Success).Item2;
            //  response.Timestamp = GetUnixTime();
        }

        private static decimal GetBalance(BllClient client, long sessionId)
        {
            decimal balance;
            var currency = client.CurrencyId;
            if (EzugiHelpers.UnsuppordedCurrenies.Contains(client.CurrencyId))
                currency = Constants.Currencies.USADollar;
            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
            if (isExternalPlatformClient)
            {
                ClientBll.GetClientPlatformSession(client.Id, sessionId);
                var balanceOutput = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, balanceOutput);
            }
            else
            {
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance);
            }
            return (long)(balance * 100) / 100m;
        }

        private static SessionIdentity CheckSession(string token, int? uId = null, bool checkExpiration = true)
        {
            var response = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, checkExpiration: checkExpiration);
            if (response.Id != uId && uId != null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.UserNotFound);
            return response;
        }
        private void CheckHash(int partnerId, string context)
        {
            if (Request.Headers.ContainsKey("hash"))
            {
                var hash = Request.Headers["hash"];
                if (!string.IsNullOrEmpty(hash))
                {
                    var secret = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EzugiSecretKey);
                    if (CreateToken(context, secret).ToLower() != hash.ToString().ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                }
            }
        }

        private static string CreateToken(string message, string secret)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }

        private static long GetUnixTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static string SplitCreditSeatId(string seatId)
        {
            var splitArray = seatId.Split('-');
            return splitArray.Length > 2 ? (splitArray[0] + "-" + splitArray[2]) : splitArray[0];
        }
    }
}