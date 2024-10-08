namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiBonusInput
    {
        public int ClientId { get; set; }
        public int BonusId { get; set; }
        public int BonusType { get; set; }
        public int? ReuseNumber { get; set; }
        public int ClientBonusId { get; set; }
        public int TriggerId { get; set; }
        public decimal SourceAmount { get; set; }
        public int Status { get; set; }
    }
}