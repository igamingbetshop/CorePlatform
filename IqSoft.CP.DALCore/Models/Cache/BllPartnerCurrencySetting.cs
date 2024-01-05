using System;
namespace IqSoft.CP.DAL.Models.Cache
{
	public class BllPartnerCurrencySetting
	{
		public int Id { get; set; }
		public int PartnerId { get; set; }
		public string CurrencyId { get; set; }
		public int State { get; set; }
		public decimal? UserMinLimit { get; set; }
		public decimal? UserMaxLimit { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
        public int? Priority { get; set; }
		public decimal? ClientMinBet { get; set; }
	}
}
