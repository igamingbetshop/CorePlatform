using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class BetMakersHelpers
    {
        public static string GetUrl(int clientId, string token, int partnerId, bool isForDemo, SessionIdentity session)
        {
            var balances = CacheManager.GetClientCurrentBalance(clientId);
            var partner = CacheManager.GetPartnerById(partnerId);
            var input = new
            {
                loggedIn = !isForDemo,
                accessToken = token,
                balance = Math.Round(balances.Balances.Where(x => x.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                  x.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                                  x.TypeId != (int)AccountTypes.ClientBonusBalance)
                                                      .Sum(x => x.Balance), 2),
                bonusBalance = Math.Round(balances.Balances.Where(x => x.TypeId == (int)AccountTypes.ClientBonusBalance)
                                                           .Sum(x => x.Balance), 2),
                showBalance = true,
                brandName = partner.Name,
                brandUserId = clientId.ToString(),
                userId = CommonFunctions.GenerateGuidFromNumber(clientId).ToString()
            };

            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
            var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
            var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(input));
            return string.Format("{0}/betmakers/launchgame?data={1}", distributionUrl, data);
        }
    }
}
