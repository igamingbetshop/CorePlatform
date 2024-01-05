using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Integration.Products.Models.GoldenRace;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class GoldenRaceHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.GoldenRace);
        public static List<GameItem> GetGames(int partnerId)
        {
            var siteId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GoldenRaceSiteId);
            var publicKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GoldenRacePublicKey);
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GoldenRaceApiUrl);
            var input = new
            {
                siteId,
                publicKey
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = string.Format("{0}get-game-list", apiUrl),
                PostData = CommonFunctions.GetUriEndocingFromObject(input)
            };
            var result = JsonConvert.DeserializeObject<GameModel>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (!result.Status)
                throw new System.Exception(result.Messsage);
            return result.Items;
        }
    }
}
