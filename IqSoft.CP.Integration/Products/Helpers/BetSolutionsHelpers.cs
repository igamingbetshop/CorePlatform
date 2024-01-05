using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.BetSolutions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public class BetSolutionsHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BetSolutions);
        public static List<GameItem> GetGames(int partnerId)
        {
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetSolutionsMerchantId);
            var secureKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetSolutionsSecureKey);
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetSolutionsApiUrl);
            var input = new
            {
                MerchantId = merchantId,
                Hash = CommonFunctions.ComputeSha256(string.Format("{0}|{1}", merchantId, secureKey))
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}Game/GetGameList", apiUrl),
                PostData = JsonConvert.SerializeObject(input)
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);

            return JsonConvert.DeserializeObject<Game>(res).Data.Products.SelectMany(x => x.Games).ToList();
        }
    }
}

