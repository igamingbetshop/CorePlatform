using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using static IqSoft.CP.Integration.Products.Helpers.InternalHelpers;
using IqSoft.CP.DAL.Models.Clients;
using System.Collections.Generic;
using System.Linq;
using System;
using log4net.Repository.Hierarchy;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public class ExternalPlatformHelpers
    {
        public static BllClient CreateClientSession(LoginInput input, out string newToken, out int clientId, SessionIdentity sessionIdentity, ILog log)
        {
            switch (input.ExternalPlatformType.Value)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    var externalClient = CashCenterHelpers.LoginUser(input.PartnerId, input.Identifier, sessionIdentity, log);
                    if (externalClient != null)
                    {
                        using (var clientBll = new ClientBll(sessionIdentity, LogManager.GetLogger("ADONetAppender")))
                        {
                            var clientIdentifier = Constants.ExternalClientPrefix + externalClient.UserId.ToString();
                            var client = CacheManager.GetClientByUserName(input.PartnerId, clientIdentifier);
                            if (client == null)
                            {
                                var newClient = clientBll.RegisterClient(new Client
                                {
                                    CurrencyId = externalClient.CurrencyId,
                                    UserName = clientIdentifier,
                                    PartnerId = input.PartnerId,
                                    Gender = (int)Gender.Male,
                                    //BirthDate = Convert.ToDateTime(externalClient.BirthDate),
                                    FirstName = externalClient.UserName
                                });
                                client = CacheManager.GetClientById(newClient.Id);
                            }
                            clientId = client.Id;
                            var session = ClientBll.CreateNewPlatformSession(client.Id, input.LanguageId, string.IsNullOrEmpty(input.Ip) ? Constants.DefaultIp : input.Ip, null,
                                string.Empty, input.DeviceType, input.Source, externalClient.Token);
                            newToken = session.Token;
                            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
                            client.CurrencySymbol = currency.Symbol;
                            return client;
                        }
                    }
                    else
                    {
                        BllClient client = null;
                        if (ClientBll.IsValidEmail(input.Identifier))
                        {
                            client = CacheManager.GetClientByEmail(input.PartnerId, input.Identifier.ToLower());
                        }
                        else if (ClientBll.IsMobileNumber(input.Identifier))
                        {
                            input.Identifier = "+" + input.Identifier.Replace("+", string.Empty).Replace(" ", string.Empty);
                            client = CacheManager.GetClientByMobileNumber(input.PartnerId, input.Identifier);
                        }
                        else
                            client = CacheManager.GetClientByUserName(input.PartnerId, input.Identifier);
                        if (client == null)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.WrongLoginParameters);
                        var resp = ClientBll.LoginClient(input, client, out newToken,out _, log);
                        clientId = client.Id;

                        return resp;
                    }

                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
        }

        public static List<Common.Models.WebSiteModels.AccountModel> GetExternalAccounts(BllClient client, SessionIdentity sessionIdentity, ILog log)
        {
            var result = new List<Common.Models.WebSiteModels.AccountModel>();
            var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.ExternalAccountsPlatformTypes);
            if (partnerConfig != null)
            {
                var platformIds = partnerConfig.Split(',')?.Select(Int32.Parse)?.ToList();
                platformIds.ForEach(platform =>
                {
                    switch (platform)
                    {
                        case (int)ExternalPlatformTypes.EveryMatrix:
                            var bonusBalance = Products.Helpers.EveryMatrixHelpers.GetPlayerBonusBalance(client, log);
                            var accountTypesNames = CacheManager.GetAccountTypes(sessionIdentity.LanguageId);
                            result.Add(new Common.Models.WebSiteModels.AccountModel
                            {
                                TypeId = (int)AccountTypes.ClientBonusBalance,
                                Balance = bonusBalance,
                                CurrencyId = client.CurrencyId,
                                AccountTypeName = accountTypesNames.FirstOrDefault(x=>x.NickName == AccountTypes.ClientBonusBalance.ToString())?.Name
                            });
                            break;
                        default:
                            break;
                    }
                });
            }
            return result;
        }

        public static decimal GetClientBalance(int externalPlatformType, int clientId, long? sessionId = null)
        {
            var client = CacheManager.GetClientById(clientId);
            var session = sessionId.HasValue ? CacheManager.GetClientPlatformSessionById(sessionId.Value) : ClientBll.GetClientPlatformSession(clientId); 

            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    var externalClient = CashCenterHelpers.GetBalance(client.PartnerId, client.UserName);
                    if (externalClient.CurrencyId != client.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    return externalClient.AvailableBalance;
                case (int)ExternalPlatformTypes.IQSoft:
                    var url = string.IsNullOrEmpty(session.CurrentPage) ?
                        CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue : session.CurrentPage;
                    var balanceOutput = Common.Helpers.CommonFunctions.SendHttpRequest(
                    new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "GetBalance"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        PostData = JsonConvert.SerializeObject(
                        new
                        {
                            Token = !string.IsNullOrEmpty(session.ExternalToken) ? session.ExternalToken : session.Token,
                            CurrencyId = client.CurrencyId
                        })
                    }, out _);
                    return JsonConvert.DeserializeObject<GetBalanceOutput>(balanceOutput).AvailableBalance;
                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
        }

        public static bool IsExternalPlatformClient(BllClient client, out DAL.Models.Cache.PartnerKey externalPlatformType)
        {
            externalPlatformType = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatform);
            return (externalPlatformType != null && externalPlatformType.NumericValue != null &&
                externalPlatformType.NumericValue.Value == (int)PartnerTypes.ExternalPlatform && client.UserName.Contains(Constants.ExternalClientPrefix));
        }

        public static decimal CreditFromClient(int externalPlatformType, BllClient client, long? sessionId, ListOfOperationsFromApi input, Document doc, ILog log)
        {
            decimal balance = 0;
            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    if (doc.Amount > 0)
                    {
                        var ccResp = CashCenterHelpers.Credit(client.Id, doc.Id.ToString(), doc.Amount);
                        balance = ccResp?.UserWalletInfo?.BalanceAfter ?? 0;
                    }
                    else
                    {
                        var eBalance = CashCenterHelpers.GetBalance(client.PartnerId, client.UserName);
                        balance = eBalance.AvailableBalance;
                    }
                    break;
                case (int)ExternalPlatformTypes.IQSoft:
                    var platformSession = ClientBll.GetClientPlatformSession(client.Id, sessionId);
                    var url = string.IsNullOrEmpty(platformSession.CurrentPage) ?
                        CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue : platformSession.CurrentPage;

                    var request = new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "Credit"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        PostData = JsonConvert.SerializeObject(
                            new
                            {
                                Token = !string.IsNullOrEmpty(platformSession.ExternalToken) ? platformSession.ExternalToken : platformSession.Token,
                                TypeId = (int)BetTypes.Single,
                                CurrencyId = input.CurrencyId,
                                RoundId = input.RoundId,
                                GameId = input.ProductId,
                                TransactionId = doc.Id,
                                OperationTypeId = (int)OperationTypes.Bet,
                                Info = input.Info,
                                BetState = (int)BetStatus.Uncalculated,
                                Amount = doc.Amount,
                                PossibleWin = doc.PossibleWin
                            })
                    };
                    var output = Common.Helpers.CommonFunctions.SendHttpRequest(request, out _);
                    var iqResp = JsonConvert.DeserializeObject<FinOperationOutput>(output);
                    if (iqResp.ResponseCode != Constants.SuccessResponseCode)
                    {
                        log.Info("UnsuccessfullResponse_Credit_" + client.PartnerId + "_" + JsonConvert.SerializeObject(request) + "_" + output);
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, iqResp.ResponseCode);
                    }
                    balance = iqResp.OperationItems[0].Balance;
                    break;
                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
            return balance;
        }

        public static decimal DebitToClient(int externalPlatformType, BllClient client, long? betId, ListOfOperationsFromApi input, Document doc, ILog log)
        {
            decimal balance = 0;
            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    if (doc.Amount > 0)
                    {
                        var ccResp = CashCenterHelpers.Debit(client.Id, doc.Id.ToString(), doc.Amount, log);
                        balance = ccResp?.UserWalletInfo?.BalanceAfter ?? 0;
                    }
                    else
                    {
                        var eBalance = CashCenterHelpers.GetBalance(client.PartnerId, client.UserName);
                        balance = eBalance.AvailableBalance;
                    }
                    break;
                case (int)ExternalPlatformTypes.IQSoft:
                    var lastSession = CacheManager.GetClientLastLoginIp(client.Id);
                    var url = string.IsNullOrEmpty(lastSession.CurrentPage) ?
                              CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue : lastSession.CurrentPage;

                    var request = new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "Debit"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        PostData = JsonConvert.SerializeObject(
                            new
                            {
                                ClientId = client.UserName.Replace(Constants.ExternalClientPrefix, string.Empty),
                                UserName = client.UserName.Replace(Constants.ExternalClientPrefix, string.Empty),
                                GameId = input.ProductId,
                                OperationTypeId = (int)OperationTypes.Win,
                                CurrencyId = input.CurrencyId,
                                RoundId = input.RoundId,
                                TransactionId = doc.Id,
                                Info = input.Info,
                                Amount = doc.Amount,
                                BetState = doc.Amount > 0 ? (int)BetStatus.Won : (int)BetStatus.Lost,
                                CreditTransactionId = betId
                            })
                    };
                    var output = Common.Helpers.CommonFunctions.SendHttpRequest(request, out _);
					var resp = JsonConvert.DeserializeObject<FinOperationOutput>(output);
                    if (resp.ResponseCode != Constants.SuccessResponseCode)
                    {
                        log.Info("UnsuccessfullResponse_Debit_" + client.PartnerId + "_" + JsonConvert.SerializeObject(request) + "_" + output);
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, resp.ResponseCode);
                    }
                    balance = resp.OperationItems[0].Balance;
                    break;
                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
            return balance;
        }

        public static void RollbackTransaction(int externalPlatformType, BllClient client, ListOfOperationsFromApi input, Document doc, ILog log)
        {
            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    CashCenterHelpers.Rollback(client.Id, doc.Id.ToString(), doc.OperationTypeId, doc.Amount, log);
                    break;
                case (int)ExternalPlatformTypes.IQSoft:
                    var lastSession = CacheManager.GetClientLastLoginIp(client.Id);
                    var url = string.IsNullOrEmpty(lastSession.CurrentPage) ?
                              CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue : lastSession.CurrentPage;

                    var request = new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "Rollback"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        PostData = JsonConvert.SerializeObject(
                            new
                            {
                                UserName = client.UserName.Replace(Constants.ExternalClientPrefix, string.Empty),
                                GameId = input.ProductId,
                                OperationTypeId = input.OperationTypeId,
                                input.GameProviderId,
                                input.ExternalProductId,
                                TransactionId = doc.Id,
                                RollbackTransactionId = doc.ParentId,
                                input.Info
                            })
                    };
                    log.Info("Rollback_Request_" + client.PartnerId + "_" + request.PostData);
                    var output = Common.Helpers.CommonFunctions.SendHttpRequest(request, out _); 
                    log.Info("Rollback_Response_" + client.PartnerId + "_" + output);

                    var resp = JsonConvert.DeserializeObject<FinOperationOutput>(output);
                    if (resp.ResponseCode != Constants.SuccessResponseCode)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, resp.ResponseCode);
                    break;
                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
        }
    }
}
