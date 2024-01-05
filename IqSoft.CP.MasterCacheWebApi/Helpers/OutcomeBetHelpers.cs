using IqSoft.CP.DAL.Models;
using log4net;
using OutcomeBet = IqSoft.CP.Integration.Products.Helpers;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class OutcomeBetHelpers
    {
        public static string GetUrl(int partnerId, int clientId, int productId, string languageId, bool isForDemo, string token, SessionIdentity  session, ILog log)
        {
            if (!isForDemo)
                return OutcomeBet.OutcomeBetHelpers.CreateSession(clientId, productId, languageId, token, session, log);
            
            return OutcomeBet.OutcomeBetHelpers.CreateDemoSession(productId, partnerId, clientId);
        }
    }
}