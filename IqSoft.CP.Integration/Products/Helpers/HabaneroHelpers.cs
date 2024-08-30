using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.Habanero;
using log4net;
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
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format(Provider.GameLaunchUrl, "ws") + "/jsonapi/GetGames",
                PostData = JsonConvert.SerializeObject(input)
            };
            return JsonConvert.DeserializeObject<GamesListOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).Games;
        }

        public static bool AddFreeRound(FreeSpinModel freeSpinModel, ILog log)
        {
            var client = CacheManager.GetClientById(freeSpinModel.ClientId);
            var brandId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.HabaneroBrandId);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.HabaneroApiKey);
            var input = new
            {
                BrandId = brandId,
                APIKey = apiKey,
                ReplaceActiveCoupon = false,
                CouponTypeId = 5,
                DtStartUTC = freeSpinModel.StartTime.ToString("yyyyMMddHHmmss"),
                DtEndUTC = freeSpinModel.FinishTime.ToString("yyyyMMddHHmmss"),
                ExpireAfterDays = Convert.ToInt32((freeSpinModel.FinishTime - freeSpinModel.StartTime).TotalDays),
                MaxRedemptionsPerPlayer = 1,
                MaxRedemptionsForBrand = 100000,
                MaxRedemptionIntervalId = 0,
                WagerMultiplierRequirement = 0,
                NumberOfFreeSpins = freeSpinModel.SpinCount,
                GameKeyNames = freeSpinModel.ProductExternalIds,
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
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format(Provider.GameLaunchUrl, "ws") + "/jsonapi/createandapplybonusmulti",
                PostData = JsonConvert.SerializeObject(input)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var freeRoundOutput = JsonConvert.DeserializeObject<FreeRoundOutput>(response);
            if (!freeRoundOutput.Created)
            {
                log.Error(response);
                return false;
            }
            return true;
        }
    }
}
