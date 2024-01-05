using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Integration.Products.Models.Nucleus;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class NucleusHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Nucleus);
        public static List<GAMESSUITESSUITEGAME> GetGames(int partnerId)
        {
            var gamesUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.NucleusApiUrl);
            var brandId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.NucleusBankId);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url =string.Format("{0}?bankId={1}", gamesUrl,brandId)
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var serializer = new XmlSerializer(typeof(GAMESSUITES), new XmlRootAttribute("GAMESSUITES"));
            var games = (GAMESSUITES)serializer.Deserialize(new StringReader(resp));
            return games.SUITES.SelectMany(x => x.GAMES.Select(y => { y.CATEGORYNAME = x.ID; return y; })).ToList();
        }
    }
}
