using System;

namespace IqSoft.CP.DAL.Models.Segment
{
    public class SegementSettingModel
    {
        public int? SegmentId { get; set; }
        public int? Priority { get; set; }
        public string SocialLink { get; set; }
        public string AlternativeDomain { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public bool? IsDefault { get; set; }
        public decimal? DepositMinAmount { get; set; }
        public decimal? DepositMaxAmount { get; set; }
        public decimal? WithdrawMinAmount { get; set; }
        public decimal? WithdrawMaxAmount { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string DomainTextTranslationKey { get; set; }

        //public static string GetConditionString(PaymentSegmentCondition conditions)
        //{
        //    if (conditions.Groups == null)
        //        return string.Empty;
        //    var cond = string.Join(conditions.GroupingType == (int)GroupingTypes.All ? " AND " : " OR ",
        //                       conditions.Conditions.Select(y => string.Format("{0}{1}{2}",
        //                                                         Enum.GetName(typeof(PaymentSegmentRules), y.ConditionType),
        //                                                         CustomHelper.ParseOperationType(y.OperationTypeId),
        //                                                         y.StringValue)));
        //    var groups = string.Join(conditions.GroupingType == (int)GroupingTypes.All ? " AND " : " OR ",
        //                       conditions.Groups.Select(x => string.Format("({0})", string.Join(x.GroupingType == (int)GroupingTypes.All ? " AND " : " OR ",
        //                                                     x.Conditions.Select(y => string.Format("{0}{1}{2}",
        //                                                     Enum.GetName(typeof(PaymentSegmentRules), y.ConditionType),
        //                                                     CustomHelper.ParseOperationType(y.OperationTypeId),
        //                                                     y.StringValue))))));
        //    if (!string.IsNullOrEmpty(cond) && !string.IsNullOrEmpty(groups))
        //        return cond + (conditions.GroupingType == (int)GroupingTypes.All ? " AND " : " OR ") + groups;
        //    return cond + groups;
        //}
    }
}