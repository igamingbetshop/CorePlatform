using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByBonus
    {
        public int PartnerId { get; set; }
        public int BonusId { get; set; }
        public string BonusName { get; set; }
        public int BonusType { get; set; }
        public bool BonusStatus { get; set; }
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public string CurrencyId { get; set; }
        public int CategoryId { get; set; }
        public decimal? BonusPrize { get; set; }
        public decimal? TurnoverAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public int ClientBonusStatus { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? AwardingTime { get; set; }
        public DateTime? CalculationTime { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? TriggerId { get; set; }
        public string TriggerName { get; set; }
        public int? TriggerType { get; set; }
        public DateTime? TriggerStartDate { get; set; }
    }
}