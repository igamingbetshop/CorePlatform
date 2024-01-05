using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Integration;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
	public static class InBetHelpers
	{
		private readonly static BllGameProvider GameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.InBet);

		public static string GetUrl(int partnerId, int productId, string token, int clientId, bool isForDemo, SessionIdentity session)
		{
			var product = CacheManager.GetProductById(productId);
			var customerId = CacheManager.GetGameProviderValueByKey(partnerId, GameProvider.Id, Constants.PartnerKeys.InBetCustomerId);
			BllClient client = null;
			if (!isForDemo)
			{
				client = CacheManager.GetClientById(clientId);
			}
			var language = CommonHelpers.LanguageISOCodes.ContainsKey(session.LanguageId) ? CommonHelpers.LanguageISOCodes[session.LanguageId] : session.LanguageId;
			return string.Format(GameProvider.GameLaunchUrl, session.Domain, product.ExternalId, customerId,
				(client == null ? "FUN" : token), 1, (client == null ? "FUN" : client.CurrencyId),
				language, session.Domain);
		}
	}
}