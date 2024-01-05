using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPartnerBetShopsOutput
    {
        public List<BetShopModel> BetShops { get; set; }

    }

    public class BetShopModel
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public string Address { get; set; }
        public int PartnerId { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int RegionId { get; set; }
    }
}