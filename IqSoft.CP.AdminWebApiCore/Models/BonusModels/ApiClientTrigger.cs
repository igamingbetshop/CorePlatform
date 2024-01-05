namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiClientTrigger
    {
        public int ClientBonusId { get; set; }
        public int  ClientId { get; set; }
        public int TriggerId { get; set; }
        public int BonusId { get; set; }
        public decimal? SourceAmount { get; set; }
        public int Status { get; set; }
        public int ReuseNumber { get; set; }
    }
}