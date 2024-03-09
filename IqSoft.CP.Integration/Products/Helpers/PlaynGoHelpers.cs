using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Products.Models.PlaynGo;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using System.Text;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.Bonus;
using System.Linq;
using log4net;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class PlaynGoHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.PlaynGo);

        public static List<GameItem> GetProductsList(int partnerId)
        {
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PlaynGoApiUrl);
            var username = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PlaynGoApiUsername);
            var pass = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PlaynGoApiPassword);
            var pid = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PlaynGoApiPId);
            var byteArray = Encoding.Default.GetBytes($"{username}:{pass}");

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = apiUrl,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) }, { "pid", pid } }
            };
            return JsonConvert.DeserializeObject<List<GameItem>>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }

        public static void AddFreeRound(FreeSpinModel freeSpinModel, ILog log)
        {
            var client = CacheManager.GetClientById(freeSpinModel.ClientId);
            var isStaging = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoGMTIsStaging);
            var username = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoGMTApiUsername);
            var pass = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoGMTApiPassword);
            if (isStaging == "0")
            {
                var prdService = new PlaynGoTPServicePrd.CasinoGameTPServiceClient();
                prdService.ClientCredentials.UserName.UserName = username;
                prdService.ClientCredentials.UserName.Password = pass;
                var gamesList = freeSpinModel.ProductExternalIds.Select(x => Convert.ToInt32(x.Split('-')[0])).ToArray();
                prdService.AddFreegameOffers(client.Id.ToString(), null, (int?)freeSpinModel.Lines, (int?)freeSpinModel.Coins, freeSpinModel.BetValueLevel,
                                          freeSpinModel.SpinCount, freeSpinModel.FinishTime, null, null, null, null, null, gamesList);

            }
            else
            {
                var stgService = new PlaynGoTPServiceStg.CasinoGameTPServiceClient();
                stgService.ClientCredentials.UserName.UserName = username;
                stgService.ClientCredentials.UserName.Password = pass;
                var gamesList = freeSpinModel.ProductExternalIds.Select(x => Convert.ToInt32(x.Split('-')[0])).ToArray();
                stgService.AddFreegameOffers(client.Id.ToString(), null, (int?)freeSpinModel.Lines, (int?)freeSpinModel.Coins, freeSpinModel.BetValueLevel,
                                          freeSpinModel.SpinCount, freeSpinModel.FinishTime, null, null, null, null, null, gamesList);
            }
        }

        public static void CancelFreeSpinBonus(int clientId, int bonusId)
        {
            var client = CacheManager.GetClientById(clientId);
            var isStaging = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoGMTIsStaging);
            var username = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoGMTApiUsername);
            var pass = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoGMTApiPassword);
            var pid = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PlaynGoApiPId);
            if (isStaging == "0")
            {
                var prdService = new PlaynGoTPServicePrd.CasinoGameTPServiceClient();
                prdService.ClientCredentials.UserName.UserName = username;
                prdService.ClientCredentials.UserName.Password = pass;
                prdService.CancelFreegame(bonusId.ToString(), Convert.ToInt32(pid));
            }
            else
            {
                var stgService = new PlaynGoTPServiceStg.CasinoGameTPServiceClient();
                stgService.ClientCredentials.UserName.UserName = username;
                stgService.ClientCredentials.UserName.Password = pass;
                stgService.CancelFreegame(bonusId.ToString(), Convert.ToInt32(pid));
            }
        }
    }
}