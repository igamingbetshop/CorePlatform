using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.Betsoft;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class BetSoftHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BetSoft);
        public static string GetSessionUrl(int partnerId, BllProduct product, string token, bool isForDemo, SessionIdentity session)
        {
            var bankId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetSoftBankId);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            if (isForDemo)
            {
                var demoInput = new
                {
                    gameId = product.ExternalId,
                    lang = session.LanguageId,
                    bankId,
                    homeUrl = casinoPageUrl
                };
                return string.Format("{0}/cwguestlogin.do?{1}", Provider.GameLaunchUrl,
                       CommonFunctions.GetUriEndocingFromObject(demoInput));
            }

            var requestInput = new
            {
                token,
                mode = "real",
                gameId = product.ExternalId,
                lang = session.LanguageId,
                bankId,
                homeUrl = casinoPageUrl
            };
            return string.Format("{0}/cwstartgamev2.do?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriDataFromObject(requestInput));
        }

        public static List<GAMESSUITESSUITEGAME> GetGames(int partnerId)
        {

            var bankId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetSoftBankId);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = string.Format("{0}/gamelist.do?bankId={1}", Provider.GameLaunchUrl, bankId)
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var serializer = new XmlSerializer(typeof(GAMESSUITES), new XmlRootAttribute("GAMESSUITES"));
            var gamesList = (GAMESSUITES)serializer.Deserialize(new StringReader(res));
              gamesList.SUITES.ForEach(x => x.GAMES.ForEach(y => y.CATEGORYID = x.ID));
            return gamesList.SUITES.SelectMany(x => x.GAMES).ToList();
        }

        public static bool AddFreeRound(FreeSpinModel freeSpinModel, ILog log)
        {
            var client = CacheManager.GetClientById(freeSpinModel.ClientId);
            var bankId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BetSoftBankId);
            var passKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BetSoftPassKey);
            var input = new
            {
                userId = freeSpinModel.ClientId,
                rounds = freeSpinModel.SpinCount,
                games = string.Join("|", freeSpinModel.ProductExternalId),
                extBonusId = freeSpinModel.BonusId,
                startTime = freeSpinModel.StartTime.ToString("dd.MM.yyyy HH:mm:ss"),
                expirationTime = freeSpinModel.FinishTime.ToString("dd.MM.yyyy HH:mm:ss")
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}/frbaward.do?bankId={1}&{2}&hash={3}", Provider.GameLaunchUrl, bankId, CommonFunctions.GetUriDataFromObject(input),
                      CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}{5}", input.userId, bankId, input.rounds, input.games, input.extBonusId, passKey)))
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var serializer = new XmlSerializer(typeof(BSGSYSTEM), new XmlRootAttribute("BSGSYSTEM"));
            var freespinOutput = (BSGSYSTEM)serializer.Deserialize(new StringReader(res));
            if (freespinOutput.RESPONSE.RESULT != "OK")
            {
                log.Error(res);
                return false;
            }
            return true;
        }
    }
}