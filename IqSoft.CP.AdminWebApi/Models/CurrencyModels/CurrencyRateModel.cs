using System;

namespace IqSoft.CP.AdminWebApi.Models.CurrencyModels
{
    public class CurrencyRateModel
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public decimal RateBefore { get; set; }
        public decimal RateAfter { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}