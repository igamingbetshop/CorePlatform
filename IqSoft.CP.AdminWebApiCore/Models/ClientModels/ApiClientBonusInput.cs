using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientBonusInput
    {
        public int ClientId { get; set; }
        public decimal? BonusAmount { get; set; }
        public int? SpinsCount { get; set; }
        public int BonusSettingId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public bool IsCampaign { get; set; }
        public string ClientData { get; set; }
        public int ReuseNumber { get; set; }
    }
}