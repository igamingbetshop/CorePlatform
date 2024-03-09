using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class SendBetSelections
	{
		public string Token { get; set; }
		public List<BetSelectionItem> BetSelections { get; set; }
	}

	public class BetSelectionItem
	{
		public int ProductId { get; set; }

		public string ProductName { get; set; }

		public long RoundId { get; set; }

		public int UnitId { get; set; }

		public string UnitName { get; set; }

		public string RoundName { get; set; }

		public int MarketTypeId { get; set; }

		public long MarketId { get; set; }

		public string MarketName { get; set; }

		public long SelectionTypeId { get; set; }

		public long SelectionId { get; set; }

		public string SelectionName { get; set; }

		public decimal Coefficient { get; set; }

		public int CombinationNumber { get; set; }

		public DateTime? EventDate { get; set; }

		public bool IsLive { get; set; }

		public string Info { get; set; }
	}
}