using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetRegionsOutput : ApiResponseBase
    {
        public List<RegionModel> Regions { get; set; }
    }

    public class RegionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string IsoCode { get; set; }
        public string IsoCode3 { get; set; }
    }
}