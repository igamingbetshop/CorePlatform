using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Integration.Products.Models.Habanero;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
   public static class HabaneroHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Habanero);
        public static List<Game> GetGames(int partnerId)
        {
            var brandId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.HabaneroBrandId);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.HabaneroApiKey);
            var input = new
            {
                BrandId = brandId,
                APIKey = apiKey
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format(Provider.GameLaunchUrl, "ws") + "/jsonapi/GetGames",
                PostData = JsonConvert.SerializeObject(input)
            };
            return JsonConvert.DeserializeObject<GamesListOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).Games;
        }

        public static void AddFreeRound(int clientId, List<string> productExternalIds, int spinCount, DateTime startTime, DateTime finishTime)
        {
            var client = CacheManager.GetClientById(clientId);
            var brandId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.HabaneroBrandId);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.HabaneroApiKey);
            var input = new
            {
                BrandId = brandId,
                APIKey = apiKey,
                ReplaceActiveCoupon = false,
                CouponTypeId = 5,
                DtStartUTC = startTime.ToString("yyyyMMddHHmmss"),
                DtEndUTC = finishTime.ToString("yyyyMMddHHmmss"),
                ExpireAfterDays = Convert.ToInt32((finishTime - startTime).TotalDays),
                MaxRedemptionsPerPlayer = 1,
                MaxRedemptionsForBrand = 100000,
                MaxRedemptionIntervalId = 0,
                WagerMultiplierRequirement = 0,
                NumberOfFreeSpins = spinCount,
                GameKeyNames = productExternalIds,
                couponCurrencyData = new List<object>{ new
                {
                    CurrencyCode = client.CurrencyId,
                    CoinPosition = 1
                } },
                CreatePlayerIfNotExist = true,
                Players = new List<object> { new { Username = client.Id.ToString(), CurrencyCode = client.CurrencyId } }
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format(Provider.GameLaunchUrl, "ws") + "/jsonapi/createandapplybonusmulti",
                PostData = JsonConvert.SerializeObject(input)
            };
            var res = JsonConvert.DeserializeObject<FreeRoundOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if(!res.Created)
                throw new Exception(res.Message);        
        }       
    }
}
