using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public int? ReuseNumber { get; set; }
	}
}
