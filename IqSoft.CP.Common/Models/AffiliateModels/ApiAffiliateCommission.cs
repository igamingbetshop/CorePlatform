using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
    public class ApiAffiliateCommission
    {
        public int AffiliateId { get; set; }
        public FixedFeeCommission FixedFeeCommission { get; set; }
        public DepositCommission DepositCommission { get; set; }
        public List<BetCommission> TurnoverCommission { get; set; }
        public List<BetCommission> GGRCommission { get; set; }
        public List<BetCommission> NGRCommission { get; set; }
    }
}
