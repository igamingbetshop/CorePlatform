using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Extensions;
using IqSoft.CP.Integration.Products.Models.GSoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace IqSoft.CP.Integration.Products.Helpers
{

    public class GSoftHelpers
    {
        private static int _providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.GSoft).Id;
        private static BllProduct _product = CacheManager.GetProductByExternalId(_providerId, "Sportsbook");
        private BllClient _client;
        private readonly string _privateKey;
        private readonly string _operatorCode;
        private readonly string _urlBase;
        private readonly string _distributionUrl;
        private static class TransferTypes
        {
            public const int Withdraw = 0;
            public const int Deposit = 1;
        };

        private enum OddsTypes
        {
            Malay = 1,
            HongKong = 2,
            Decimal = 3,
            Indo = 4,
            American = 5
        };

        private static Dictionary<int, string> Currencies { get; set; } = new Dictionary<int, string>
        {
            { 2, "MYR" }, { 4, "THB" }, { 13, "CNY" }, { 15, "IDR" }, { 32, "JPY" }, { 45, "KRW" }, { 51, "VND" }, { 20, "test"}
        };

        private static readonly Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            { 10300, Constants.Errors.DontHaveAccessToThisPartner }, { 18003, Constants.Errors.WrongHash },
            { 18004, Constants.Errors.WrongToken }, { 18005, Constants.Errors.InvalidUserName },
            { 18008, Constants.Errors.WrongOperationAmount }, { 18009, Constants.Errors.WrongOperationAmount },
            { 18024, Constants.Errors.WrongDocumentId }, { 18025, Constants.Errors.WrongOperationAmount },
            { 18103, Constants.Errors.DocumentNotFound }, { 19000, Constants.Errors.WrongInputParameters },
            { 25998, Constants.Errors.LowBalance }, { 2750001, Constants.Errors.LowBalance}
        };


        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return Constants.Errors.GeneralException;
        }

        public GSoftHelpers(BllClient client)
        {
            _client = client;
            _privateKey =  CacheManager.GetGameProviderValueByKey(client.PartnerId, _providerId, Constants.PartnerKeys.GSoftPrivateKey + client.CurrencyId);
            _operatorCode = CacheManager.GetGameProviderValueByKey(client.PartnerId, _providerId, Constants.PartnerKeys.GSoftOpCode + client.CurrencyId);
            _urlBase = CacheManager.GetGameProviderValueByKey(client.PartnerId, _providerId, Constants.PartnerKeys.GSoftUrl);
            _distributionUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, _providerId, Constants.PartnerKeys.DistributionUrl);
        }

        private string CallGSoftApi(string method, object input)
        {
            input.GetType().GetProperty("vendor_id").SetValue(input, _privateKey);
            if (method != "CheckUserBalance")
                input.GetType().GetProperty("vendor_member_id").SetValue(input, _client.Id.ToString());
            else
                input.GetType().GetProperty("vendor_member_ids").SetValue(input, _client.Id.ToString());
            var url = string.Format("{0}/{1}?{2}", _urlBase, method, CommonFunctions.GetUriEndocingFromObject(input));
            return SendRequestToProvider(url);
        }

        private T PrepareAndSendRequest<T>(string queryString)
        {
            //string securityToken =  CommonFunctions.ComputeMd5(_privateKey + queryString).ToUpper();
            var url = string.Format("{0}{1}", _urlBase, queryString);

            var res = JsonConvert.DeserializeObject<string>(SendRequestToProvider(url));
            try
            {
                var output = JsonConvert.DeserializeObject<T>(res);
                int errorCode = Convert.ToInt32(output.GetType().GetProperty("ErrorCode").GetValue(output));
                string message = output.GetType().GetProperty("Message").GetValue(output).ToString();
                if (errorCode != 0 && errorCode != 6)
                {
                    if (Errors.ContainsKey(errorCode))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Errors[errorCode]);
                    else
                        throw new Exception(errorCode.ToString() + "  " + message);
                }
                return output;
            }
            catch
            {
                throw new Exception(res);
            }
        }

        private string SendRequestToProvider(string url)
        {
            var requestData = new { Content = url };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = _distributionUrl+ "/api/GSoft/CallGSoftApi",
                PostData = JsonConvert.SerializeObject(requestData)
            };

            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        public void CreateMember()
        {
            string queryString = string.Format("/{0}?vendor_id={1}&vendor_member_id={2}&operatorId={3}&firstname={4}&lastname={5}&username={6}" +
                "&oddstype={7}&currency={8}&maxtransfer={9}&mintransfer={10}",
                MethodBase.GetCurrentMethod().Name, _privateKey,_client.Id.ToString(), _operatorCode, 
                _client.FirstName, _client.LastName,_client.Id.ToString(),
                (int)OddsTypes.Indo, Currencies.FirstOrDefault(x => x.Value == _client.CurrencyId).Key,  100000, 1);

            PrepareAndSendRequest<BaseResponse>(queryString);
        }

        public void UpdateMember()
        {
            string queryString = string.Format("/{0}?vendor_id={1}&vendor_member_id={2}&firstname=&lastname=" +
                                               "&oddstype={3}&maxtransfer={4}&mintransfer={5}",
            MethodBase.GetCurrentMethod().Name, _privateKey, _client.Id.ToString(),
            (int)OddsTypes.Indo, 100000, 1);
            PrepareAndSendRequest<BaseResponse>(queryString);
        }

        public string LogIn()
        {
            var requestInput = new BaseInput();
            var res = JsonConvert.DeserializeObject<string>(CallGSoftApi(MethodBase.GetCurrentMethod().Name, requestInput));
            var output = JsonConvert.DeserializeObject<LoginOutput>(res);
            if (output.ErrorCode != 0)
                throw new Exception("Login_" + output.Message);
            return output.Data;
        }

        public decimal CheckUserBalance()
        {
            var requestInput = new BaseInput();
            var res = JsonConvert.DeserializeObject<string>(CallGSoftApi(MethodBase.GetCurrentMethod().Name, requestInput));
            try
            {
                var output = JsonConvert.DeserializeObject<CheckPlayerBalanceOutput>(res);
                if (output.ErrorCode != 0)
                    throw new Exception(output.Message);
                if (Currencies[output.Data[0].Currency] != _client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                return output.Data[0].Balance;
            }
            catch
            {
                throw new Exception(res);
            }
        }

        public void KickUser()
        {
            var requestInput = new BaseInput();
            var res = CallGSoftApi(MethodBase.GetCurrentMethod().Name, requestInput);
            var output = JsonConvert.DeserializeObject<BaseResponse>(res);
            if (output.ErrorCode != 0)
                throw new Exception(output.Message);
        }

        public DAL.Document Transfer(ClientBll clientBl, DocumentBll documentBl, decimal amount, int direction)
        {
            if (amount <= 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
            
            var operations = new ListOfOperationsFromApi
            {
                TransactionId = Guid.NewGuid().ToString(),
                ProductId = _product.Id,
                CurrencyId = _client.CurrencyId,
                GameProviderId = _providerId,
                OperationItems = new List<OperationItemFromProduct>
                {
                    new OperationItemFromProduct { Client = _client }
                }
            };

            var docAmount = amount;
            DAL.Document betDoc = null;
            FundTransferOutput output = null;
            if (_client.CurrencyId == "IDR")
                docAmount = amount * 1000;
            if (direction == (int)OperationTypes.Bet)
            {
                operations.OperationItems[0].Amount = docAmount;
                betDoc = clientBl.CreateCreditFromClient(operations, documentBl, out LimitInfo info);
                try
                {
                    CreateMember();
                    output = FundTransfer(amount, TransferTypes.Deposit, betDoc.Id);
                }
                catch (FaultException<BllFnErrorType> fex)
                {
					documentBl.RollbackProductTransactions(operations);
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, fex.Detail.Id);
                }
                catch (Exception ex)
                {
					documentBl.RollbackProductTransactions(operations);
					throw new Exception(ex.Message);
                }
                betDoc.ExternalTransactionId = output.Data.TransactionExternalId.ToString();
				documentBl.UpdateDocumentExternalId(betDoc.Id, betDoc.ExternalTransactionId);

                operations.TransactionId = betDoc.ExternalTransactionId;
            }
            else
            {
                operations.OperationItems[0].Amount = 0;
                betDoc = clientBl.CreateCreditFromClient(operations, documentBl, out LimitInfo info);
            }
            operations.CreditTransactionId = betDoc.Id;
            operations.OperationTypeId = (int)OperationTypes.Win;
            DAL.Document winDoc = null;
            if (direction == (int)OperationTypes.Bet)
            {
                operations.State = (int)DocumentStates.Lost;
                operations.OperationItems[0].Amount = 0;
				clientBl.CreateDebitsToClients(operations, betDoc, documentBl);
            }
            else
            {
                operations.State = (int)DocumentStates.Won;
                operations.OperationItems[0].Amount = docAmount;
                winDoc = clientBl.CreateDebitsToClients(operations, betDoc, documentBl).FirstOrDefault();
                try
                {
                    output = FundTransfer(amount, TransferTypes.Withdraw, winDoc.Id);
                }
                catch (FaultException<BllFnErrorType> fex)
                {
					documentBl.RollbackProductTransactions(operations);
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, fex.Detail.Id);
                }
                catch (Exception ex)
                {
					documentBl.RollbackProductTransactions(operations);
                    throw new Exception(ex.Message);
                }
            }
            return direction == (int)OperationTypes.Bet ? betDoc : winDoc;
        }

        private FundTransferOutput FundTransfer(decimal amount, int transferType, long transactionId)
        {
            string queryString = string.Format("/{0}?vendor_id={1}&vendor_member_id={2}&vendor_trans_id={3}&amount={4}&" +
                "currency={5}&direction={6}&wallet_id=1",
                MethodBase.GetCurrentMethod().Name, _privateKey, _client.Id.ToString(), transactionId,
                amount, Currencies.FirstOrDefault(x => x.Value == _client.CurrencyId).Key, transferType);

            return PrepareAndSendRequest<FundTransferOutput>(queryString);
        }

        public FundTransferOutput CheckFundTransfer(long transactionId)
        {
            string queryString = string.Format("/{0}?vendor_id={1}&vendor_trans_id={2}&wallet_id=1",
                MethodBase.GetCurrentMethod().Name, _privateKey, transactionId);

            return PrepareAndSendRequest<FundTransferOutput>(queryString);
        }

        public GetSportBetLogOutput GetSportBetLog(long lastVersionKey)
        {
            string queryString = string.Format("/{0}?vendor_id={1}&LastVersionKey={2}&Lang={3}",
                MethodBase.GetCurrentMethod().Name, _privateKey, lastVersionKey, _client.LanguageId ?? Constants.DefaultLanguageId);

            return PrepareAndSendRequest<GetSportBetLogOutput>(queryString);
        }

        public GetSportBetLogOutput GetSportBettingDetail(DateTime startTime, DateTime endTime, int rMode = 0)
        {
            string queryString = string.Format("/{0}?vendor_id={1}&StartTime={2}&EndTime={3}",
                MethodBase.GetCurrentMethod().Name, _privateKey, startTime.EncodeDate(), endTime.EncodeDate());

            return PrepareAndSendRequest<GetSportBetLogOutput>(queryString);
        }

        public GetSportBettingMixParlayDetailOutput GetSportBettingMixParlayDetail(long parlayRefNumber)
        {
            string queryString = string.Format("/{0}?vendor_id={1}&RefNo={2}",
                MethodBase.GetCurrentMethod().Name, _privateKey, parlayRefNumber);

            return PrepareAndSendRequest<GetSportBettingMixParlayDetailOutput>(queryString);
        }

        public GetBetSettingLimitOutput GetBetSettingLimit()
        {
            string queryString = string.Format("/{0}?vendor_id={1}&Currency={2}",
                MethodBase.GetCurrentMethod().Name, _privateKey, Currencies.FirstOrDefault(c => c.Value == _client.CurrencyId).Key);

            return PrepareAndSendRequest<GetBetSettingLimitOutput>(queryString);
        }

        public GetBetSettingLimitOutput GetMemberBetSetting()
        {
            var requestInput = new BaseInput();
            var res = CallGSoftApi(MethodBase.GetCurrentMethod().Name, requestInput);
            var output = JsonConvert.DeserializeObject<GetBetSettingLimitOutput>(res);
            if (output.ErrorCode != 0)
                throw new Exception(output.Message);
            return output;
        }

        public void PrepareMemberBetSetting(SettingData settings)
        {
            string queryString = string.Format("/{0}?vendor_id={1}&vendor_member_id={2}&sportType={3}&minBet={4}&maxBet={5}&maxBetPerMatch={6}&maxBetPerBall={7}",
                MethodBase.GetCurrentMethod().Name, _privateKey, _client.Id.ToString(), settings.SportType, settings.MinBet, settings.MaxBet, settings.MaxBetPerMatch, settings.MaxBetPerBall);

            PrepareAndSendRequest<BaseResponse>(queryString);
        }

        public void ConfirmMemberBetSetting()
        {
            var requestInput = new BaseInput();
            var res = CallGSoftApi(MethodBase.GetCurrentMethod().Name, requestInput);
            var output = JsonConvert.DeserializeObject<BaseResponse>(res);
            if (output.ErrorCode != 0)
                throw new Exception(output.Message);
        }

        public GetBalanceHistoryOutput GetBalanceHistory()
        {
            string queryString = string.Format("/{0}?vendor_id={1}&vendor_member_id={2}&Date={3}",
                MethodBase.GetCurrentMethod().Name, _privateKey, _client.Id.ToString(), DateTime.UtcNow.ToString("yyyyMMdd"));

            return PrepareAndSendRequest<GetBalanceHistoryOutput>(queryString);
        }

        public GetGameDetailOutput GetGameDetail(string matchId)
        {
            string queryString = string.Format("//{0}?vendor_id={1}&Match_Id={2}",
                MethodBase.GetCurrentMethod().Name, _privateKey, matchId);

            return PrepareAndSendRequest<GetGameDetailOutput>(queryString);
        }
    }
}