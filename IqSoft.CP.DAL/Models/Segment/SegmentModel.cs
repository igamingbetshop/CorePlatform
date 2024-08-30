using IqSoft.CP.Common.Attributes;
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
        public SegmentSettingModel SegementSetting { get; set; }
        public bool? IsKYCVerified { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsMobileNumberVerified { get; set; }
        public int? Gender { get; set; }        
        public bool? IsTermsConditionAccepted { get; set; } // ???

        [PropertyCustomTypeAttribute(TypeName = "IntArray")]
        public Condition ClientStatus { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "IntArray")]
        public Condition ClientId { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "EmailArray")]
        public Condition Email { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "StringArray")]
        public Condition FirstName { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "StringArray")]
        public Condition LastName { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "IntArray")]
        public Condition Region { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition AffiliateId { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition AgentId { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "MobileArray")]
        public Condition MobileCode { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition SessionPeriod { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "DateTime")]
        public Condition SignUpPeriod { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition TotalDepositsCount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Decimal")]
        public Condition TotalDepositsAmount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition TotalWithdrawalsCount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Decimal")]
        public Condition TotalWithdrawalsAmount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition TotalBetsCount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Decimal")]
        public Condition Profit { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "IntArray")]
        public Condition SuccessDepositPaymentSystem { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "IntArray")]
        public Condition SuccessWithdrawalPaymentSystem { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Decimal")]
        public Condition ComplimentaryPoint { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition SportBetsCount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Int")]
        public Condition CasinoBetsCount { get; set; }

        [PropertyCustomTypeAttribute(TypeName = "Decimal")]
        public Condition TotalBetsAmount { get; set; }
    }  
}
