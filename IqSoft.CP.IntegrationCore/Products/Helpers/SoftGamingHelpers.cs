using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.SoftGaming;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class SoftGamingHelpers
    {
        private static List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.ArgentinianPeso,
            Constants.Currencies.ColumbianPeso,
            Constants.Currencies.IranianTuman,
            Constants.Currencies.USDT
        };

        private static List<string> NotSupportedLanguages = new List<string>
        {
           "uz"
        };

        private static readonly List<string> NonDirectProviders = new List<string>
        {
            "netent",
            "playngo",
            "tomhorn",
            "spigo"
        };

        private static readonly Dictionary<int, string> OperatorsList = new Dictionary<int, string>
        {
            { 998,"EvoSW" },
            { 997,"MG" },
            { 992,"NetEnt" },
            { 991,"BetSoft" },
            { 987,"TomHorn" },
            { 986,"MrSlotty" },
            { 984,"Playson" },
            { 977,"BoomingGames" },
            { 976,"Habanero" },
            { 975,"AmaticDirect" },
            { 969,"Quickspin" },
            { 963,"ISoftBet" },
            { 960,"PragmaticPlay" },
            { 959,"Spinomenal" },
            { 957,"Spigo" },
            { 956,"Belatra" },
            { 954,"EGTInteractive" },
            { 953,"Yggdrasil" },
            { 949,"Platipus" },
            { 948,"Edict" },
            { 944,"PlaynGo" },
            { 943,"PlaysonDirect" },
            { 941,"Wazdan" },
            { 940,"EvoPlay" },
            { 939,"PGSoft" },
            { 938,"NoLimitCity" },
            { 935,"RelaxGaming" },
            { 933,"RedTigerGaming" },
            { 931,"Leander" },
            { 930,"Genii" },
            { 927,"Fugaso" },
            { 926,"Igrosoft" },
            { 925,"ELKStudios" },
            { 924,"Booongo" },
            { 920,"Thunderkick" },
            { 917,"Stakelogic" },
            { 914,"BeeFee" },
            { 912,"SolidGaming" },
            { 910,"RedRakeGaming" }
        };

        private static readonly List<string> RedirectingProviders = new List<string>
        {
            "microgaming",
            "isoftbet",
            "playtech"
        };

        public static string GetUrl(int partnerId, int productId, string token, int clientId,
            bool isForDemo, bool isForMobile, SessionIdentity session, ILog logger)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftGaming);
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var partner = CacheManager.GetPartnerById(partnerId);
            var productData = product.ExternalId.Split(',');
            var method = "DirectAuth";
            var subProvider = string.Empty;
            var hostName = Dns.GetHostName();
            var currency = client != null ? client.CurrencyId : partner.CurrencyId;
            if (NotSupportedCurrencies.Contains(currency))
                currency = Constants.Currencies.USADollar;
            if (product.SubProviderId.HasValue)
            {
                subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value).Name.ToLower();

                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
                if (NonDirectProviders.Contains(subProvider))
                {
                    method = "AuthHTML";
                    hostName = string.Format(distributionUrlKey.StringValue, session.Domain);
                }
                else if (RedirectingProviders.Contains(subProvider))
                    hostName = string.Format(distributionUrlKey.StringValue, session.Domain);
            }
            var serverIPs = Dns.GetHostEntry(hostName.Replace("https://", string.Empty).Replace("http://", string.Empty)).AddressList;
            var ip = serverIPs[serverIPs.Length - 1].ToString();
            var ipMap = CacheManager.GetConfigParameters(Constants.MainPartnerId, "MasterServerIpMap");
            if (ipMap.Any())
            {
                foreach (var item in ipMap)
                {
                    if (item.Key.IsIpEqual(ip))
                    {
                        ip = item.Value;
                        break;
                    }
                }
            }
            var casinoPageUrl = CacheManager.GetGameProviderValueByKey(partnerId, product.SubProviderId ?? gameProvider.Id, Constants.PartnerKeys.CasinoPageUrl);
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var input = new LaunchInput
            {
                Login = "us" + client?.Id,
                Password = "p000000",
                TID = CommonFunctions.GetRandomString(30),
                Currency = currency,
                Language = NotSupportedLanguages.Contains(session.LanguageId.ToLower()) ? Constants.DefaultLanguageId : session.LanguageId.ToLower(),
                UserIP = ip,
                Demo = isForDemo ? 1 : 0,
                IsMobile = isForMobile ? 1 : 0,
                ExtParam = token,
                Page = !isForMobile ? productData[1] : productData[2],
                System = productData[0],
                UserAutoCreate = 1,
                Referer = casinoPageUrl
            };

            var apiKey = CacheManager.GetGameProviderValueByKey(isForDemo ? partnerId : client.PartnerId, gameProvider.Id, Constants.PartnerKeys.SoftGamingApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(isForDemo ? partnerId : client.PartnerId, gameProvider.Id, Constants.PartnerKeys.SoftGamingApiPwd);

            var hash = CommonFunctions.ComputeMd5(string.Format("User/{7}/{0}/{1}/{2}/{3}/{4}/{5}/{6}", input.UserIP, input.TID, apiKey, input.Login, input.Password, productData[0], pwd, method));
            var url = string.Format("{0}/System/Api/{1}/User/{4}?{2}&Hash={3}", gameProvider.GameLaunchUrl, apiKey,
                CommonFunctions.GetUriEndocingFromObject(input), hash, method);
            if (method == "AuthHTML")
            {
                input.UserIP = string.Empty;
                url = string.Format("{0}/System/Api/{1}/User/{2}", gameProvider.GameLaunchUrl, apiKey, method);
                var redirectedIp = CacheManager.GetConfigKey(Constants.MainPartnerId, Constants.PartnerKeys.DistributionAddress);
                hash = CommonFunctions.ComputeMd5(string.Format("User/{7}/{0}/{1}/{2}/{3}/{4}/{5}/{6}", redirectedIp, input.TID, apiKey, input.Login, input.Password, productData[0], pwd, method));
                return string.Format("{0}/softgaming/launchgame?requestUrl={1}&inputData={2}&hash={3}", hostName, url, JsonConvert.SerializeObject(input), hash);
            }
            if (RedirectingProviders.Contains(subProvider))
            {
                input.UserIP = session.LoginIp;
                url = string.Format("{0}/System/Api/{1}/User/{2}", gameProvider.GameLaunchUrl, apiKey, method);
                var redirectedIp = CacheManager.GetConfigKey(Constants.MainPartnerId, Constants.PartnerKeys.DistributionAddress);
                hash = CommonFunctions.ComputeMd5(string.Format("User/{7}/{0}/{1}/{2}/{3}/{4}/{5}/{6}", redirectedIp, input.TID, apiKey, input.Login, input.Password, productData[0], pwd, method));
                url = string.Format("{0}/softgaming/redirectgame?requestUrl={1}&inputData={2}&hash={3}", hostName, url, JsonConvert.SerializeObject(input), hash);
            }
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = url
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var res = response.Split(',');
            if (res.Count() == 2)
                return res[1];

            logger.Info("GetUrlOutput_" + response);
            return res[0];
        }

        public static List<GameItem> GetGamePage(int partnerid, out List<MerchantItem> subProviders)
        {
            string hostName = Dns.GetHostName();
            var serverIPs = Dns.GetHostEntry(hostName.Replace("https://", string.Empty).Replace("http://", string.Empty)).AddressList;
            var ip = serverIPs[serverIPs.Length - 1].ToString();
            var ipMap = CacheManager.GetConfigParameters(Constants.MainPartnerId, "MasterServerIpMap");
            if (ipMap.Any())
            {
                foreach (var item in ipMap)
                {
                    if (item.Key.IsIpEqual(ip))
                    {
                        ip = item.Value;
                        break;
                    }
                }
            }
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftGaming);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerid, gameProvider.Id, Constants.PartnerKeys.SoftGamingApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(partnerid, gameProvider.Id, Constants.PartnerKeys.SoftGamingApiPwd);
            var tid = CommonFunctions.GetRandomString(30);
            var hash = string.Format("Game/FullList/{0}/{1}/{2}/{3}", ip, tid, apiKey, pwd);
            hash = CommonFunctions.ComputeMd5(hash);
            var url = string.Format("{0}/System/Api/{1}/Game/FullList/?TID={2}&Hash={3}", gameProvider.GameLaunchUrl, apiKey, tid, hash);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = url
            };
            var res = JsonConvert.DeserializeObject<GamesList>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            subProviders = res.merchants.Values.Select(x =>
            {
                var name = x.Name.Replace("AmaticDirect", "Amatic").Replace("BigTime", "BigTimeGaming")
                                 .Replace("EvoSW", "Evolution").Replace("Relax", "RelaxGaming"); ;
                if (name.ToLower() == "mg")
                    name = "microgaming";
                return new MerchantItem
                {
                    ID = x.ID,
                    Name = name
                };
            }).ToList();
            return res.games;
        }

        public static void AddFreeRound(int clientId, string productExternalId, int spinCount, DateTime finishTime)
        {
            var client = CacheManager.GetClientById(clientId);
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftGaming);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.SoftGamingApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.SoftGamingApiPwd);
            var externalData = productExternalId.Split(',');
            var operatorId = Convert.ToInt32(externalData[0]);
            if (!OperatorsList.ContainsKey(operatorId))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
            var input = new
            {
                Operator = OperatorsList[operatorId],
                Login = "us" + client.Id,
                Games = externalData[1],
                Count = spinCount,
                Expire = finishTime.ToString("yyyy-MM-dd HH:mm:ss"),
                TID = CommonFunctions.GetRandomString(30),
            };
            var hostName = Dns.GetHostName();
            var serverIPs = Dns.GetHostEntry(hostName.Replace("https://", string.Empty).Replace("http://", string.Empty)).AddressList;
            var ip = (serverIPs[serverIPs.Length - 1]).ToString();
            var hash = string.Format("{0}/Freerounds/{1}/{2}/{3}/{4}", input.Operator, ip, input.TID, apiKey, pwd);
            var url = string.Format("{0}/System/Api/{1}/Freerounds/Add?{2}&Hash={3}", gameProvider.GameLaunchUrl, apiKey,
                CommonFunctions.GetUriEndocingFromObject(input), CommonFunctions.ComputeMd5(hash));

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = HttpMethod.Get,
                Url = url
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            if (res != "1")
                throw new Exception(JsonConvert.SerializeObject(res));
        }
    }
}