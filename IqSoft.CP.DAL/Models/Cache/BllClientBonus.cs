using System;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BllClientBonus
	{
		public int Id { get; set; }
		public int BonusId { get; set; }
		public int ClientId { get; set; }
		public int Status { get; set; }
		public decimal BonusPrize { get; set; }
		public DateTime CreationTime { get; set; }
		public decimal? TurnoverAmountLeft { get; set; }
		public decimal? FinalAmount { get; set; }
		public DateTime? CalculationTime { get; set; }
		public long? ReuseNumber { get; set; }
	}
}
