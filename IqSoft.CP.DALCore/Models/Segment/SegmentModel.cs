using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.DAL.Models.Segment
{
    public class SegmentModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public int? State { get; set; }
        public int Mode { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public SegementSettingModel SegementSetting { get; set; }
        public bool? IsKYCVerified { get; set; }
        public int? Gender { get; set; }        
        public bool? IsTermsConditionAccepted { get; set; } // ???
        public Condition ClientStatus { get; set; }
        public Condition SegmentId { get; set; }
        public Condition ClientId { get; set; }
        public Condition Email { get; set; }
        public Condition FirstName { get; set; }
        public Condition LastName { get; set; }
        public Condition Region { get; set; }
        public Condition AffiliateId { get; set; }
        public Condition MobileCode { get; set; }
        public Condition SessionPeriod { get; set; }
        public Condition SignUpPeriod { get; set; }
        public Condition TotalDepositsCount { get; set; } 
        public Condition TotalDepositsAmount { get; set; }
        public Condition TotalWithdrawalsCount { get; set; }
        public Condition TotalWithdrawalsAmount { get; set; }
        public Condition TotalBetsCount { get; set; } 
        public Condition Profit { get; set; }
        public Condition Bonus { get; set; } // ????
        public Condition SuccessDepositPaymentSystem { get; set; }
        public Condition SuccessWithdrawalPaymentSystem { get; set; }
        public Condition ComplimentaryPoint { get; set; } 
        public Condition SportBetsCount { get; set; }
        public Condition CasinoBetsCount { get; set; }
        public Condition TotalBetsAmount { get; set; }
    }  
}
