namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EvenBetHelpers
    {
        public static string GetUrl(int partnerId, int clientId, string languageId, bool isForDemo)
        {
            return  Integration.Products.Helpers.EvenBetHelpers.GetSessionUrl(partnerId, clientId, isForDemo, languageId);
        }       
    }
}