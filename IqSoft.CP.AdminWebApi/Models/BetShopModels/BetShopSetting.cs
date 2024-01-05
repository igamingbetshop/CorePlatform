namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class BetShopSetting
    {
        public int? MaxCopyCount { get; set; }
        public decimal? MaxWinAmount { get; set; }
        public decimal? MinBetAmount { get; set; }
        public int? MaxEventCountPerTicket { get; set; }
        public int? CommissionType { get; set; }
        public decimal? CommissionRate { get; set; }
        public bool? AnonymousBet { get; set; }
        public bool? AllowCashout { get; set; }
        public bool? AllowLive { get; set; }
        public bool? UsePin { get; set; }
        public string ExternalId { get; set; } // to be removed
    }
}