using System;
using System.Collections.Generic;
using System.Web.Http;
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
using System.Web;
using System.Text;
using System.Security.Cryptography;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Net.Http;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class EzugiController : ApiController
    {
        private readonly static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Ezugi).Id;
        public static readonly List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Ezugi);
        

        [HttpPost]
        [Route("{partnerId}/api/Ezugi/Authentication")]
        public AuthenticationOutput Authentication(AuthenticationInput input)
        {
            var response = new AuthenticationOutput();
            BllClient client;
            SessionIdentity clientSession;
            try
            {
                try
                {
                    BaseBll.CheckIp(WhitelistedIps);
                }
                catch
                {
                    WebApiApplication.DbLogger.Info("NewIp: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
                clientSession = CheckSession(input.Token);
                client = CacheManager.GetClientById(clientSession.Id);
                CheckHash(client.PartnerId, null);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
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
                    response.Balance = GetBalance(client, product.Id, clientSession.ParentId ?? 0);
                    response.Timestamp = GetUnixTime();
                }
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
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
        public async Task<IHttpActionResult> Credit(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            var input = JsonConvert.DeserializeObject<DebitInput>(inputString);
            var response = new DebitOutput
            {
                ClientId = input.ClientId.ToString(),
                OperatorId = input.OperatorId,
                RoundId = input.RoundId,
                Token = input.Token,
                TransactionId = input.TransactionId,
                Currency = input.Currency,
                Timestamp = GetUnixTime()
            };
            var clientSession = new SessionIdentity();
            var client = CacheManager.GetClientById(input.ClientId);
            try
            {
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                try
                {
                    BaseBll.CheckIp(WhitelistedIps);
                }
                catch
                {
                    WebApiApplication.DbLogger.Info("NewIp: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
                clientSession = CheckSession(input.Token, input.ClientId);
                CheckHash(client.PartnerId, inputString);
                await Task.Run(() => ProcessCredit(input, clientSession, response));
                response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
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
                    response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound && error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
                }
            }
            response.Timestamp = GetUnixTime();
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Ezugi/Credit")]
        public async Task<IHttpActionResult> Debit( HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            var input = JsonConvert.DeserializeObject<CreditInput>(inputString);
            var response = new CreditOutput
            {
                OperatorId = input.OperatorId,
                RoundId = input.RoundId,
                ClientId = input.ClientId.ToString(),
                Token = input.Token,
                TransactionId = input.TransactionId,
                Currency = input.Currency,
                Timestamp = GetUnixTime(),
            };
            var clientSession = new SessionIdentity();
            var client = CacheManager.GetClientById(input.ClientId);
            try
            {
                try
                {
                    BaseBll.CheckIp(WhitelistedIps);
                }
                catch
                {
                    WebApiApplication.DbLogger.Info("NewIp: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
                clientSession = CheckSession(input.Token, input.ClientId, false);
                CheckHash(client.PartnerId, inputString);
                await Task.Run(() => ProcessDebit(input, clientSession, response));
                response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
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
                    response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);

                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + "_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound && error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
                }
            }
            response.Timestamp = GetUnixTime();
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/Ezugi/Rollback")]
        public BaseOutput Rollback(RollbackInput input)
        {
            var response = new RollbackOutput
            {
                OperatorId = input.OperatorId,
                RoundId = input.RoundId,
                ClientId = input.ClientId.ToString(),
                Token = input.Token,
                TransactionId = input.TransactionId,
                Currency = input.Currency,
                Timestamp = GetUnixTime(),
            };
            var clientSession = new SessionIdentity();
            var client = CacheManager.GetClientById(input.ClientId);
            try
            {
                try
                {
                    BaseBll.CheckIp(WhitelistedIps);
                }
                catch
                {
                    WebApiApplication.DbLogger.Info("NewIp: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
                clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                CheckHash(client.PartnerId, null);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
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
                                    ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), 
                                        client, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex.Message);
                                }
                            }
                        }
                        catch(FaultException<BllFnErrorType>)
                        {
                            CacheManager.SetFutureRollback(Constants.CacheItems.EzugiRollback, input.TransactionId, input.TransactionId);
                            throw;
                        }
                        response.Balance = GetBalance(client, product.Id, clientSession.ParentId ?? 0);                       
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        response.ErrorCode = EzugiHelpers.ErrorCodes.Success;
                        response.ErrorDescription = EzugiHelpers.GetError(EzugiHelpers.ErrorCodes.Success).Item2;
                    }
                }
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
                    response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
                }
                WebApiApplication.DbLogger.Error(fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("_" + ex.Message);
                var error = EzugiHelpers.GetError(Constants.Errors.GeneralException);
                response.ErrorCode = error.Item1;
                response.ErrorDescription = error.Item2;
                if (error.Item1 != EzugiHelpers.ErrorCodes.TokenNotFound && error.Item1 != EzugiHelpers.ErrorCodes.UserNotFound)
                {
                    response.ClientId = input.ClientId.ToString();
                    response.Balance = GetBalance(client, clientSession.ProductId, clientSession.ParentId ?? 0);
                }
            }
            response.Timestamp = GetUnixTime();
            return response;
        }

        private void ProcessCredit(DebitInput input, SessionIdentity clientSession, DebitOutput response)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
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
                    var doc = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                    BaseHelpers.BroadcastBetLimit(info);
                    var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.EzugiRollback, input.TransactionId);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);

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
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                var balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, 
                                    operationsFromProduct, doc, WebApiApplication.DbLogger);
                                BaseHelpers.BroadcastBalance(client.Id, balance);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                    }
                    if (!isExternalPlatformClient)
                    {
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    response.ClientId = client.Id.ToString();
                 //   response.Balance = GetBalance(client, clientSession.ParentId ?? 0);                  
                    response.ErrorDescription = EzugiHelpers.GetError(EzugiHelpers.ErrorCodes.Success).Item2;
                  //  response.Timestamp = GetUnixTime();
                }
            }
        }

        private void ProcessDebit(CreditInput input, SessionIdentity clientSession, CreditOutput response)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
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
                                                                  (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0], WebApiApplication.DbLogger);
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
                            WebApiApplication.DbLogger.Error("DebitException_" + JsonConvert.SerializeObject(message));
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastWin(new ApiWin
                    {
                        BetId = betDocument?.Id ?? 0,
                        GameName = partnerProductSetting.NickName,
                        ClientId = client.Id,
                        ClientName = client.FirstName,
                        BetAmount = betDocument?.Amount,
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
            }
        }
      
        private decimal GetBalance(BllClient client, int productId, long sessionId)
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
                balance = BaseBll.ConvertCurrency(client.CurrencyId, currency, BaseHelpers.GetClientProductBalance(client.Id, productId));
            }
            return (long)(balance * 100) / 100m;
        }

        private SessionIdentity CheckSession(string token, int? uId = null, bool checkExpiration = true)
        {
            var response = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, checkExpiration: checkExpiration);
            if (response.Id != uId && uId != null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.UserNotFound);
            return response;
        }
        private void CheckHash(int partnerId, string context)
        {
            if (Request.Headers.Contains("hash"))
            {
                var hash = Request.Headers.GetValues("hash").FirstOrDefault();
                if(!string.IsNullOrEmpty(hash) )
                {
                    if (string.IsNullOrEmpty(context))
                    using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                    {
                        context = reader.ReadToEnd();
                    }
                    var secret = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EzugiSecretKey);
                    if (CreateToken(context, secret).ToLower() != hash.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                }
            }
        }

        private string CreateToken(string message, string secret)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
        private long GetUnixTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private string SplitCreditSeatId(string seatId)
        {
            var splitArray = seatId.Split('-');
            return splitArray.Length > 2 ? (splitArray[0] + "-" + splitArray[2]) : splitArray[0];
        }
    }
}