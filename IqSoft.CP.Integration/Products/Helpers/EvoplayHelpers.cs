using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.Evoplay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EvoplayHelpers
    {
        private static BllGameProvider GameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Evoplay);
        public static Dictionary<int, GameItem> GetGames(int partnerId)
        {
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, GameProvider.Id, Constants.PartnerKeys.EvoplayApiKey);
            var projectId = CacheManager.GetGameProviderValueByKey(partnerId, GameProvider.Id, Constants.PartnerKeys.EvoplayProjectId);

            var sign = CommonFunctions.ComputeMd5(string.Format("{0}*1*{1}", projectId, apiKey));
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = string.Format("{0}/Game/getList?project={1}&version=1&signature={2}", GameProvider.GameLaunchUrl, projectId, sign)
            };
            var res = JsonConvert.DeserializeObject<GameList>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.Status.ToLower() != "ok")
                throw new Exception("error");
            return res.Data;
        }

        public static string GetSignatureString(object source)
        {
            var properties = new List<string>();
            var generic = new List<string>();

            foreach (var p in source.GetType().GetProperties())
            {
                var val = p.GetValue(source, null);
                if (val == null)
                    continue;
                if (p.PropertyType.IsGenericType || p.PropertyType.Name == "TransactionDetails")
                {
                    generic = new List<string>();
                    foreach (var subP in val.GetType().GetProperties())
                    {
                        var subVal = subP.GetValue(val, null);
                        if (subVal == null)
                            continue;
                        generic.Add(subVal.ToString());
                    }
                    properties.Add(string.Join(":", generic.ToArray().Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '=')));
                }
                else
                {
                    properties.Add(val.ToString());
                }

            }
            return string.Join("*", properties.ToArray().Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '='));
        }
    }
}
