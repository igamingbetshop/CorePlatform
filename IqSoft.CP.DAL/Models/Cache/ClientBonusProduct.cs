using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class ClientBonusProduct
	{
		public int Id { get; set; }
		public int BonusId { get; set; }
		public int ProductId { get; set; }
		public decimal? Percent { get; set; }
		public int? Count { get; set; }
		public decimal? Lines { get; set; }
		public decimal? Coins { get; set; }
		public decimal? CoinValue { get; set; }
		public string BetValues { get; set; }
	}
}
