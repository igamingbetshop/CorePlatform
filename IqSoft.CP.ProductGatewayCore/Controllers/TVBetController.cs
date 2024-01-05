using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.TVBet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class TVBetController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "31.186.86.108",
            "18.192.80.100"
            //"91.225.165.30" testing
        };

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TVBet).Id;

        [HttpPost]
        [Route("{partnerId}/api/TVBet/GetUserData")]
        public ActionResult GetUserData(int partnerId, BaseInput input)
        {
            Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
            var signature = input.Signature;
            input.Signature = null;
            var secureKey = string.Empty;

            var response = new BaseModel
            {
                UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                ErrorDescription = string.Empty,
                ClientVal = new ClientData()
            };

            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                // BaseBll.CheckIp(WhitelistedIps, ip);
                secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TVBetSecureKey);
                var stringToSign = JsonConvert.SerializeObject(input, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + secureKey;
                var sign = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(stringToSign));
                if (signature != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                response.IsSuccess = true;
                response.ResultCode = 0;
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                decimal balance;

                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                response.ClientVal = new ClientData
                {
                    ClientId = client.Id.ToString(),
                    Currency = client.CurrencyId,
                    Token = clientSession.Token,
                    Balance = Math.Round(balance, 2).ToString(),
                    IsTest = false
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.IsSuccess = false;
                if (fex.Detail != null)
                {
                    response.ResultCode = TVBetHelpers.GetError(fex.Detail.Id);
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                    response.ErrorDescription = fex.Message;
                }
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "Error:" + fex.Detail.Message);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                response.ErrorDescription = ex.Message;
                Program.DbLogger.Error(ex);
            }
            var resp = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var baseOutput = new BaseOutput
            {
                UnixTime = response.UnixTime,
                IsSuccess = response.IsSuccess,
                ResultCode = response.ResultCode,
                ErrorDescription = response.ErrorDescription,
                ClientVal = response.ClientVal,
                Signature = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(resp + secureKey))
            };
            Program.DbLogger.Info("Output: " + JsonConvert.SerializeObject(baseOutput));
            return Ok(baseOutput);
        }

        [HttpPost]
        [Route("{partnerId}/api/TVBet/RefreshToken")]
        public ActionResult RefreshToken(int partnerId, BaseInput input)
        {
            var signature = input.Signature;
            input.Signature = null;
            var secureKey = string.Empty;

            var response = new BaseModel
            {
                UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                ErrorDescription = string.Empty,
                ClientVal = new ClientData()
            };

            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    //BaseBll.CheckIp(WhitelistedIps, ip);
                    secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TVBetSecureKey);
                    var stringToSign = JsonConvert.SerializeObject(input, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + secureKey;
                    var sign = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(stringToSign));
                    if (signature != sign)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                    var newSession = clientBl.RefreshClientSession(clientSession.Token, true).Token;
                    BaseHelpers.RemoveSessionFromeCache(clientSession.Token, null);

                    response.IsSuccess = true;
                    response.ResultCode = 0;
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    response.ClientVal = new ClientData
                    {
                        ClientId = client.Id.ToString(),
                        Currency = client.CurrencyId,
                        Token = newSession,
                        IsTest = false
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.IsSuccess = false;
                if (fex.Detail != null)
                {
                    response.ResultCode = TVBetHelpers.GetError(fex.Detail.Id); ;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                    response.ErrorDescription = fex.Message;
                }
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "Error:" + fex.Detail.Message);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                response.ErrorDescription = ex.Message;
                Program.DbLogger.Error(ex);
            }
            var resp = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var baseOutput = new BaseOutput
            {
                UnixTime = response.UnixTime,
                IsSuccess = response.IsSuccess,
                ResultCode = response.ResultCode,
                ErrorDescription = response.ErrorDescription,
                ClientVal = response.ClientVal,
                Signature = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(resp + secureKey))
            };
            return Ok(baseOutput);
        }

        [HttpPost]
        [Route("{partnerId}/api/TVBet/MakePayment")]
        public ActionResult MakePayment(int partnerId, TransactionInput input)
        {
            var signature = input.Signature;
            input.Signature = null;
            var secureKey = string.Empty;

            var response = new BaseModel
            {
                UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                IsSuccess = false,
                ResultCode = TVBetHelpers.ErrorCodes.InvalidTypeOfTransaction,
                ErrorDescription = string.Empty,
                ClientVal = new ClientData()
            };

            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                // BaseBll.CheckIp(WhitelistedIps, ip);
                secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TVBetSecureKey);

                var stringToSign = JsonConvert.SerializeObject(input, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + secureKey;
                var sign = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(stringToSign));

                if (signature != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                switch (input.TransactionType)
                {
                    case (int)TVBetHelpers.TransactionTypes.Bet:
                        response = DoBet(input);
                        break;
                    case (int)TVBetHelpers.TransactionTypes.Win:
                        response = DoWin(input);
                        break;
                    case (int)TVBetHelpers.TransactionTypes.Refund:
                    case (int)TVBetHelpers.TransactionTypes.RefundWin:
                        response = Refund(input);
                        break;
                    case (int)TVBetHelpers.TransactionTypes.JackpotPayout:
                        response = JackpotWin(input);
                        break;
                    default:
                        break;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.IsSuccess = false;
                if (fex.Detail != null)
                {
                    response.ResultCode = TVBetHelpers.GetError(fex.Detail.Id);
                    response.ErrorDescription = fex.Detail.Message;
                    if (fex.Detail.Id == Constants.Errors.SessionExpired || fex.Detail.Id == Constants.Errors.SessionNotFound)
                        response.ClientVal = null;
                }
                else
                {
                    response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                    response.ErrorDescription = fex.Message;
                }
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                response.ErrorDescription = ex.Message;
                Program.DbLogger.Error(ex);
            }
            var resp = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var baseOutput = new BaseOutput
            {
                UnixTime = response.UnixTime,
                IsSuccess = response.IsSuccess,
                ResultCode = response.ResultCode,
                ErrorDescription = response.ErrorDescription,
                ClientVal = response.ClientVal,
                Signature = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(resp + secureKey))
            };
            return Ok(baseOutput);
        }

        private static BaseModel DoBet(TransactionInput input)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.ExtensionData.Games[0].GameExternalId.ToString());
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id,
                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            RoundId  = input.ExtensionData.Games[0].RoundId,
                            TransactionId = input.TransactionId.ToString(),
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = Convert.ToDecimal(input.Amount),
                            DeviceTypeId = clientSession.DeviceType
                        });
                        document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);

                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, document);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        return new BaseModel
                        {
                            UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                            IsSuccess = true,
                            ResultCode = TVBetHelpers.ErrorCodes.Success,
                            ErrorDescription = string.Empty,
                            ClientVal = new ClientData
                            {
                                TransactionId = document.Id.ToString(),
                                TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                            }
                        };
                    }
                    return new BaseModel
                    {
                        UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                        IsSuccess = false,
                        ResultCode = TVBetHelpers.ErrorCodes.TransactionAlreadyExists,
                        ErrorDescription = "Transaction already exists",
                        ClientVal = new ClientData
                        {
                            TransactionId = document.Id.ToString()
                        }
                    };
                }
            }
        }

        private static BaseModel DoWin(TransactionInput input)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null, false);
            if (clientSession == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            long transId = 0;
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    var product = CacheManager.GetProductByExternalId(ProviderId, input.ExtensionData.Games[0].GameExternalId.ToString());
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                        product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);


                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(),
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument != null)
                        return new BaseModel
                        {
                            UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                            IsSuccess = false,
                            ResultCode = TVBetHelpers.ErrorCodes.TransactionAlreadyExists,
                            ErrorDescription = "Transaction already exists",
                            ClientVal = new ClientData
                            {
                                TransactionId = winDocument.Id.ToString(),
                                TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                            }
                        };

                    var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(),
                            client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                    var amount = Convert.ToDecimal(input.Amount);
                    var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                    betDocument.State = state;

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalOperationId = null,
                        ProductId = betDocument.ProductId,
                        TransactionId = input.TransactionId.ToString(),
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        Info = string.Empty,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = Convert.ToDecimal(input.Amount)
                    });
                    var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                    transId = doc[0].Id;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                  betDocument.Id, operationsFromProduct, doc[0]);
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

                    return new BaseModel
                    {
                        UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                        IsSuccess = true,
                        ResultCode = TVBetHelpers.ErrorCodes.Success,
                        ErrorDescription = string.Empty,
                        ClientVal = new ClientData
                        {
                            TransactionId = transId.ToString(),
                            TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                        }
                    };
                }
            }
        }

        private static BaseModel Refund(TransactionInput input)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null, false);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var client = CacheManager.GetClientById(clientSession.Id);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ExtensionData.Games[0].GameExternalId.ToString());
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    SessionId = clientSession.SessionId,
                    GameProviderId = ProviderId,
                    TransactionId = input.TransactionId.ToString(),
                    ProductId = product.Id
                };
                var operationType = input.TransactionType == (int)TVBetHelpers.TransactionTypes.Refund ? (int)OperationTypes.Bet : (int)OperationTypes.Win;
                var document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id, ProviderId, partnerProductSetting.Id, operationType);

                if (document == null || document.State == (int)BetDocumentStates.Deleted)
                    return new BaseModel
                    {
                        UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                        IsSuccess = false,
                        ResultCode = TVBetHelpers.ErrorCodes.TransactionAlreadyExists,
                        ErrorDescription = "Transaction already exists",
                        ClientVal = new ClientData
                        {
                            TransactionId = document?.Id.ToString(),
                            TransactionTime = document != null ? Convert.ToInt64((document.CreationTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds) : null
                        }
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
                document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id, ProviderId, partnerProductSetting.Id, operationType);
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
                return new BaseModel
                {
                    UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                    IsSuccess = true,
                    ResultCode = TVBetHelpers.ErrorCodes.Success,
                    ErrorDescription = string.Empty,
                    ClientVal = new ClientData
                    {
                        TransactionId = document?.Id.ToString(),
                        TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                    }
                };
            }
        }

        private static BaseModel JackpotWin(TransactionInput input)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null, false);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.ExtensionData.Games[0].GameExternalId.ToString());
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var transactionId = "JackpotWin_" + input.TransactionId.ToString();
                    var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                        return new BaseModel
                        {
                            UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                            IsSuccess = false,
                            ResultCode = TVBetHelpers.ErrorCodes.TransactionAlreadyExists,
                            ErrorDescription = "Transaction already exists",
                            ClientVal = new ClientData
                            {
                                TransactionId = winDocument.Id.ToString(),
                                TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                            }
                        };

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        TransactionId = "JackpotBet_" + input.TransactionId.ToString(),
                        OperationTypeId = (int)OperationTypes.Bet,
                        State = (int)BetDocumentStates.Uncalculated,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = 0
                    });
                    var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                    betDocument.State = (int)BetDocumentStates.Won;
                    operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ProductId = product.Id,
                        TransactionId = transactionId,
                        CreditTransactionId = betDocument.Id,
                        State = (int)BetDocumentStates.Won,
                        Info = string.Empty,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = Convert.ToDecimal(input.Amount)
                    });

                    winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                  betDocument.Id, operationsFromProduct, winDocument);
                        }
                        catch 
                        {
                            documentBl.RollbackProductTransactions(operationsFromProduct);
                            throw;
                        }
                    }
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
                    return new BaseModel
                    {
                        UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                        IsSuccess = true,
                        ResultCode = TVBetHelpers.ErrorCodes.Success,
                        ErrorDescription = string.Empty,
                        ClientVal = new ClientData
                        {
                            TransactionId = winDocument.Id.ToString(),
                            TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                        }
                    };

                }
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/TVBet/GetPaymentInfo")]
        public ActionResult GetPaymentInfo(int partnerId, GetPaymentInfo input)
        {
            var signature = input.Signature;
            input.Signature = null;
            var secureKey = string.Empty;

            var response = new BaseModel
            {
                UnixTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                ErrorDescription = string.Empty,
                ClientVal = new ClientData()
            };

            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TVBetSecureKey);
                var stringToSign = JsonConvert.SerializeObject(input, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + secureKey;
                var sign = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(stringToSign));

                if (signature != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var document = documentBl.GetDocumentById(Convert.ToInt64(input.TransactionId));
                    if (document == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    response.IsSuccess = true;
                    response.ResultCode = TVBetHelpers.ErrorCodes.Success;
                    var operationType = document.OperationTypeId == (int)OperationTypes.Bet ? -1 : 1;
                    if (document.State == (int)BetDocumentStates.Deleted && document.OperationTypeId == (int)OperationTypes.Bet)
                        operationType = 2;
                    else if (document.State == (int)BetDocumentStates.Deleted && document.OperationTypeId == (int)OperationTypes.Win)
                        operationType = -2;
                    else if (document.ExternalTransactionId.Contains("Jackpot"))
                        operationType = 4;
                    var externalId = document.ExternalTransactionId;
                    var ind = externalId.IndexOf("_");
                    if (ind != -1)
                        externalId = externalId.Substring(ind + 1, externalId.Length - ind - 1);
                    response.ClientVal = new ClientData
                    {
                        TransactionExternalId = Convert.ToInt64(externalId),
                        TransactionTime = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                        TransactionType = operationType,
                        ClientId = clientSession.Id.ToString(),
                        Amount = document.Amount.ToString("0.#")
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                response.IsSuccess = false;
                if (fex.Detail != null)
                {
                    response.ResultCode = TVBetHelpers.GetError(fex.Detail.Id); ;
                    response.ErrorDescription = fex.Detail.Message;
                }
                else
                {
                    response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                    response.ErrorDescription = fex.Message;
                }
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "Error:" + fex.Detail.Message);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResultCode = TVBetHelpers.ErrorCodes.InternalError;
                response.ErrorDescription = ex.Message;
                Program.DbLogger.Error(ex + "Error:" + ex.Message);
            }
            var resp = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var baseOutput = new BaseOutput
            {
                UnixTime = response.UnixTime,
                IsSuccess = response.IsSuccess,
                ResultCode = response.ResultCode,
                ErrorDescription = response.ErrorDescription,
                ClientVal = response.ClientVal,
                Signature = Convert.ToBase64String(CommonFunctions.ComputeMd5Bytes(resp + secureKey))
            };
            return Ok(baseOutput);
        }
    }
}