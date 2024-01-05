using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.AdminModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiBonus
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public int? AccountTypeId { get; set; }
        public bool Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public DateTime LastExecutionTime { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int? Period { get; set; }
        public decimal? Percent { get; set; }
        public List<ApiBonusProducts> Products { get; set; }
        public int BonusTypeId { get; set; }
        public string Info { get; set; }
        public int? TurnoverCount { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int? Sequence { get; set; } //??
        public string PromoCode { get; set; } //??
        public int? Priority { get; set; } //?
        public bool? IgnoreEligibility { get; set; }
        public int? ValidForAwarding {get; set; }
        public int? ValidForSpending { get; set; }
        public int? ReusingMaxCount { get; set; }
        public bool? ResetOnWithdraw { get; set; }
        public bool? RefundRollbacked { get; set; }
        public bool? AllowSplit { get; set; }
        public ApiSetting Countries { get; set; }
        public ApiSetting Languages { get; set; }
        public ApiSetting Currencies { get; set; }
        public ApiSetting SegmentIds { get; set; }
        public ApiSetting PaymentSystemIds { get; set; }
        public bool? LinkedCampaign { get; set; } //?
        public BonusCondition Conditions { get; set; }
        public decimal? MaxGranted { get; set; }
        public decimal TotalGranted { get; set; }
        public int? MaxReceiversCount { get; set; }
        public int TotalReceiversCount { get; set; }
        public int? LinkedBonusId { get; set; } //?
        public decimal? AutoApproveMaxAmount { get; set; } //?
        public bool? FreezeBonusBalance { get; set; }
    }
}