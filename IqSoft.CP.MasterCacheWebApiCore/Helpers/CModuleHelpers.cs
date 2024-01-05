using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using System.Collections.Generic;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
	public static class CModuleHelpers
	{
		private static readonly BllGameProvider GameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.CModule);
		private static readonly List<string> LowRateCurrencies = new List<string>
		{
			Constants.Currencies.IranianRial,
			Constants.Currencies.IranianTuman,
			Constants.Currencies.ChileanPeso,
			Constants.Currencies.KoreanWon
		};
		private static readonly List<string> NotSupportedCurrencies = new List<string>
		{
			Constants.Currencies.USDT
		};
		public static string GetUrl(BllPartner partner, int productId, string token, int clientId, bool isForMobile, bool isForDemo, SessionIdentity session)
		{
			var product = CacheManager.GetProductById(productId);
			var provider = "netend";
			var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partner.Id, Constants.PartnerKeys.CasinoPageUrl).StringValue;
			if (string.IsNullOrEmpty(casinoPageUrl))
				casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
			else
				casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

			if (product.SubProviderId.HasValue)
			{
				provider = CacheManager.GetGameProviderById(product.SubProviderId.Value).Name.ToLower();
				if (provider.Contains("pragmatic"))
					provider = "pragmatic";
			}
			var client = isForDemo ? new BllClient() : CacheManager.GetClientById(clientId);
			var currency = isForDemo ? partner.CurrencyId : client.CurrencyId;
			var balance = isForDemo ? 50000 :
				(long)(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * (LowRateCurrencies.Contains(client.CurrencyId) ? 1 : 100));
			if (NotSupportedCurrencies.Contains(client.CurrencyId))
				balance = (long)BaseBll.ConvertCurrency(currency, Constants.Currencies.USADollar, balance);

			currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : currency;
			var partnerKey = CacheManager.GetPartnerSettingByKey(partner.Id, Constants.PartnerKeys.CModulePartnerId).StringValue;

			return string.Format("{0}{1}?partner.alias={2}&game.alias={3}&partner.session={4}&currency={5}&balance={6}&lang={7}&mobile={8}&game.provider={9}&lobby_url={10}",
				GameProvider.GameLaunchUrl, isForDemo ? "Demo" : string.Empty,
				partnerKey, product.NickName, (isForDemo ? string.Empty : token), currency,
				balance, session.LanguageId, isForMobile, provider, casinoPageUrl);
		}
	}
}