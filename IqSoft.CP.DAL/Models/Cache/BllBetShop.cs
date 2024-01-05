using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllBetShop
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public int PartnerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
		public bool PrintLogo { get; set; }
        public string Ips { get; set; }
        public int GroupId { get; set; }
        public int Type { get; set; }
        public int RegionId { get; set; }
        public int State { get; set; }
        public decimal DefaultLimit { get; set; }
        public decimal? BonusPercent { get; set; }
        public int? UserId { get; set; }
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
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime{ get; set; }
    }
}
