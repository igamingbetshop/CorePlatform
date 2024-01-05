using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApiCore.Models
{
    public class AppConfigurationModel
    {
        public  string BetShopConnectionUrl;
        public  List<string> WhitelistedCountries { get; private set; }
        public  List<string> BlockedIps { get; private set; }
        public  List<string> WhitelistedIps { get; private set; }
    }
}
