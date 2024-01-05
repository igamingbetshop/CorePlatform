using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EvolutionHelpers
    {
        public static string GetUrl(int productId, string token, int clientId, bool isForMobile, string ip, SessionIdentity session)
        {
			return Integration.Products.Helpers.EvolutionHelpers.GetUrl(productId, token, clientId, isForMobile, ip, session, WebApiApplication.DbLogger);
        }
    }
}