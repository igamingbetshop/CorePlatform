using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using System;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using static IqSoft.CP.Integration.Products.Helpers.InternalHelpers;
using IqSoft.CP.DAL.Models.Clients;
using System.Net.Http;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public class ExternalPlatformHelpers
    {
        public static BllClient CreateClientSession(ClientLoginInput input, out string newToken, out int clientId, SessionIdentity sessionIdentity, ILog log)
        {
            switch (input.ExternalPlatformType.Value)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    var externalClient = CashCenterHelpers.LoginUser(input.PartnerId, input.ClientIdentifier, sessionIdentity, log);
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
                        if (ClientBll.IsValidEmail(input.ClientIdentifier))
                        {
                            client = CacheManager.GetClientByEmail(input.PartnerId, input.ClientIdentifier.ToLower());
                        }
                        else if (ClientBll.IsMobileNumber(input.ClientIdentifier))
                        {
                            input.ClientIdentifier = "+" + input.ClientIdentifier.Replace("+", string.Empty).Replace(" ", string.Empty);
                            client = CacheManager.GetClientByMobileNumber(input.PartnerId, input.ClientIdentifier);
                        }
                        else
                            client = CacheManager.GetClientByUserName(input.PartnerId, input.ClientIdentifier);
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

        public static decimal GetClientBalance(int externalPlatformType, int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            var session = ClientBll.GetClientPlatformSession(clientId);

            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    var externalClient = CashCenterHelpers.GetBalance(client.PartnerId, client.UserName);
                    if (externalClient.CurrencyId != client.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    return externalClient.AvailableBalance;
                case (int)ExternalPlatformTypes.IQSoft:
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue;
                    var balanceOutput = Common.Helpers.CommonFunctions.SendHttpRequest(
                    new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "GetBalance"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = HttpMethod.Post,
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

        public static decimal CreditFromClient(int externalPlatformType, BllClient client, long? sessionId, ListOfOperationsFromApi input, Document doc)
        {
            decimal balance = 0;
            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    var ccResp = CashCenterHelpers.Credit(client.Id, doc.Id.ToString(), doc.Amount);
                    balance = ccResp?.UserWalletInfo?.BalanceAfter ?? 0;
                    break;
                case (int)ExternalPlatformTypes.IQSoft:
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue;
                    var platformSession = ClientBll.GetClientPlatformSession(client.Id, sessionId);
                    var output = Common.Helpers.CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "Credit"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = HttpMethod.Post,
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
                    }, out _);
                    var iqResp = JsonConvert.DeserializeObject<FinOperationOutput>(output);
                    if (iqResp.ResponseCode != Constants.SuccessResponseCode)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, iqResp.ResponseCode);
                    balance = iqResp.OperationItems[0].Balance;
                    break;
                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
            return balance;
        }

        public static decimal DebitToClient(int externalPlatformType, BllClient client, long? betId, ListOfOperationsFromApi input, Document doc)
        {
            decimal balance = 0;
            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    if (doc.Amount > 0)
                    {
                        var ccResp = CashCenterHelpers.Debit(client.Id, doc.Id.ToString(), doc.Amount);
                        balance = ccResp?.UserWalletInfo?.BalanceAfter ?? 0;
                    }
                    break;
                case (int)ExternalPlatformTypes.IQSoft:
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue;
                    var output = Common.Helpers.CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "Debit"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = HttpMethod.Post,
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
                    }, out _);
                    var resp = JsonConvert.DeserializeObject<FinOperationOutput>(output);
                    if (resp.ResponseCode != Constants.SuccessResponseCode)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, resp.ResponseCode);
                    balance = resp.OperationItems[0].Balance;
                    break;
                default:
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            }
            return balance;
        }

        public static void RollbackTransaction(int externalPlatformType, BllClient client, ListOfOperationsFromApi input, Document doc)
        {
            switch (externalPlatformType)
            {
                case (int)ExternalPlatformTypes.CashCenter:
                    CashCenterHelpers.Rollback(client.Id, doc.Id.ToString(), doc.OperationTypeId, doc.Amount);
                    break;
                case (int)ExternalPlatformTypes.IQSoft:
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue;
                    var output = Common.Helpers.CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
                    {
                        Url = string.Format(url, "Rollback"),
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = HttpMethod.Post,
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
                    }, out _);
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
