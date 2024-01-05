using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class TwoWinPowerHelpers
    {
        public static string GetUrl(int partnerId, int productId, string token, int clientId,bool isForDemo, SessionIdentity session)
        {
            return Integration.Products.Helpers.TwoWinPowerHelpers.GetSessionUrl(partnerId, clientId, token, productId, isForDemo, session);
        }
    }
}