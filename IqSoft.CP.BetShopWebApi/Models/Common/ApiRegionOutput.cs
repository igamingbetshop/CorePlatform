using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class ApiRegionOutput : ClientRequestResponseBase
    {
        public List<RegionItem> Entities { get; set; }
    }
    public class RegionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string IsoCode { get; set; }
        public string IsoCode3 { get; set; }
    }
}