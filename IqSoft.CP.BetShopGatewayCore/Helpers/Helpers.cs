using System;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.BetShopGatewayWebApi.Models;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Interfaces;
using System.Threading.Tasks;
using IqSoft.CP.BetShopGatewayCore;

namespace IqSoft.CP.BetShopGatewayWebApi.Helpers
{
    public static class Helpers
    {
        private readonly static string LoginHashDefaultPass = "LoginHashDefaultPassword";
        private readonly static string LoginHashDefaultSalt = "LoginHashDefaultSalt";
        private readonly static string LoginHashDefaultIv = "2121212121212121";

        public static AuthorizationBase CheckCashDeskHash(int partnerId, string hash, IBetShopBll betShopBl)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            var encryption = new RijndaelEncrypt(LoginHashDefaultPass, LoginHashDefaultSalt, LoginHashDefaultIv);
            //default keys
            string decryptedHashStr = encryption.Decrypt(hash);
            var authBase = JsonConvert.DeserializeObject<AuthorizationBase>(decryptedHashStr);
            var cashDesk = CacheManager.GetCashDeskById(authBase.CashDeskId);
            if (cashDesk == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
            if (betShop.PartnerId != partnerId)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPartnerId);
            encryption = new RijndaelEncrypt(cashDesk.EncryptPassword, cashDesk.EncryptSalt, cashDesk.EncryptIv);
            //cashDesk keys
            var cashDeskDataStr = encryption.Decrypt(authBase.CashDeskData);
            authBase.Data = JsonConvert.DeserializeObject<CashDeskData>(cashDeskDataStr);
            var currentTime = betShopBl.GetServerDate();
            if (Math.Abs((authBase.Data.Date - currentTime).TotalSeconds) < 600 && cashDesk.MacAddress == authBase.Data.MacAddress)
                return authBase;

            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
        }

        public static CardReaderAuthorizationInput CheckCardReaderHash(int partnerId, string hash, IBetShopBll betShopBl)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            var encryption = new RijndaelEncrypt(LoginHashDefaultPass, LoginHashDefaultSalt, LoginHashDefaultIv);
            //default keys
            string decryptedHashStr = encryption.Decrypt(hash);
            var authBase = JsonConvert.DeserializeObject<CardReaderAuthorizationInput>(decryptedHashStr);
            var cashDesk = CacheManager.GetCashDeskById(authBase.CashDeskId);
            if (cashDesk == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
            if (betShop.PartnerId != partnerId)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPartnerId);
            encryption = new RijndaelEncrypt(cashDesk.EncryptPassword, cashDesk.EncryptSalt, cashDesk.EncryptIv);
            //cashDesk keys
            var cashDeskDataStr = encryption.Decrypt(authBase.CashDeskData);
            authBase.Data = JsonConvert.DeserializeObject<CardReaderData>(cashDeskDataStr);
            var currentTime = betShopBl.GetServerDate();
            var strCurrentTime = Convert.ToInt64(currentTime.Year* 10000000000 + currentTime.Month * 100000000 + currentTime.Day* 1000000 +
                currentTime.Hour* 10000 + currentTime.Minute*100 + currentTime.Second);
            if (Math.Abs((authBase.Data.Date - strCurrentTime)) < 1000 && cashDesk.MacAddress == authBase.Data.MacAddress)
                return authBase;

            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
        }

        public static void InvokeMessage(string messageName, params object[] obj)
        {
            Task.Run(() => Program.JobHubProxy.Invoke(messageName, obj));
        }    
    }
}