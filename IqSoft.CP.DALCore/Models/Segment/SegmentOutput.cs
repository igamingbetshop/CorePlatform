using IqSoft.CP.Common.Models.Filters;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.DAL.Models.Segment
{
    public class SegmentOutput
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public int State { get; set; }
        public int Mode { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public SegementSettingModel SegementSetting { get; set; }
        public bool? IsKYCVerified { get; set; }
        public int? Gender { get; set; }
        public bool? IsTermsConditionAccepted { get; set; } // ???
        public string ClientStatus { get; set; }
        [JsonIgnore]
        public Condition ClientStatusObject { get; set; }
        public string SegmentId { get; set; }
        [JsonIgnore]
        public Condition SegmentIdObject { get; set; }
        public string ClientId { get; set; }
        [JsonIgnore]
        public Condition ClientIdObject { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public Condition EmailObject { get; set; }
        public string FirstName { get; set; }
        [JsonIgnore]
        public Condition FirstNameObject { get; set; }
        public string LastName { get; set; }
        [JsonIgnore]
        public Condition LastNameObject { get; set; }
        public string Region { get; set; }
        [JsonIgnore]
        public Condition RegionObject { get; set; }
        public string AffiliateId { get; set; }
        [JsonIgnore]
        public Condition AffiliateIdObject { get; set; }
        public string MobileCode { get; set; }
        [JsonIgnore]
        public Condition MobileCodeObject { get; set; }
        public string SessionPeriod { get; set; }
        [JsonIgnore]
        public Condition SessionPeriodObject { get; set; }
        public string SignUpPeriod { get; set; }
        [JsonIgnore]
        public Condition SignUpPeriodObject { get; set; }
        public string Profit { get; set; }
        [JsonIgnore]
        public Condition ProfitObject { get; set; }
        public string Bonus { get; set; }
        [JsonIgnore]
        public Condition BonusObject { get; set; } // ????
        public string SuccessDepositPaymentSystem { get; set; }
        [JsonIgnore]
        public Condition SuccessDepositPaymentSystemObject { get; set; }
        public string SuccessWithdrawalPaymentSystem { get; set; }
        [JsonIgnore]
        public Condition SuccessWithdrawalPaymentSystemObject { get; set; }
        public string TotalBetsCount { get; set; }
        [JsonIgnore]
        public Condition TotalBetsCountObject { get; set; }
        public string SportBetsCount { get; set; }
        [JsonIgnore]
        public Condition SportBetsCountObject { get; set; }
        public string CasinoBetsCount { get; set; }
        [JsonIgnore]
        public Condition CasinoBetsCountObject { get; set; }
        public string TotalBetsAmount { get; set; }
        [JsonIgnore]
        public Condition TotalBetsAmountObject { get; set; }
        public string TotalDepositsCount { get; set; }
        [JsonIgnore]
        public Condition TotalDepositsCountObject { get; set; }
        public string TotalDepositsAmount { get; set; }
        [JsonIgnore]
        public Condition TotalDepositsAmountObject { get; set; }
        public string TotalWithdrawalsCount { get; set; }
        [JsonIgnore]
        public Condition TotalWithdrawalsCountObject { get; set; }
        public string TotalWithdrawalsAmount { get; set; }
        [JsonIgnore]
        public Condition TotalWithdrawalsAmountObject { get; set; }
        public string ComplimentaryPoint { get; set; }
        [JsonIgnore]
        public Condition ComplimentaryPointObject { get; set; }
    }
}
