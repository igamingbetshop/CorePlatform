using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Bets
{
	public class ApiBetItem
	{
		public int UnitId { get; set; }

		public string RoundId { get; set; }

		public int MatchId { get; set; }

		public long MarketId { get; set; }

		public int MarketTypeId { get; set; }

		public long SelectionId { get; set; }

		public string SelectionName { get; set; }

		public decimal Coefficient { get; set; }

		public string UnitName { get; set; }

		public string MarketName { get; set; }

		public DateTime EventDate { get; set; }

		public int ResponseCode { get; set; }

		public string Description { get; set; }

		public int Status { get; set; }
		
		public string State { get; set; }
		
		public string StatusName { get; set; }
	}
}