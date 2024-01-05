using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiRestrictionModel
    {
        public List<string> WhitelistedCountries { get; set; }
        public List<string> BlockedCountries { get; set; }
        public List<string> WhitelistedIps { get; set; }
        public List<string> BlockedIps { get; set; }
        public int? RegistrationLimitPerDay { get; set; }
        public string ConnectingIPHeader { get; set; }
        public string IPCountryHeader { get; set; }
    }
}
