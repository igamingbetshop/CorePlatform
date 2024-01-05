using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PlaceBetInput
	{
		public int CashierId { get; set; }

		public int CashDeskId { get; set; }

		public decimal Amount { get; set; }

		public int AcceptType { get; set; }

		public int BetType { get; set; }

		public int Type { get; set; }

		public int? SystemOutCount { get; set; }

		public List<PlaceBetInputItem> Bets { get; set; }
	}

	public class PlaceBetInputItem
	{
		public int GameId { get; set; }

		public string Token { get; set; }

		public List<PlaceBetInputItemElement> Events { get; set; }
	}

	public class PlaceBetInputItemElement
	{
		public int UnitId { get; set; }

		public int RoundId { get; set; }

		public int MarketTypeId { get; set; }

		public long MarketId { get; set; }

		public long SelectionId { get; set; }

		public long SelectionTypeId { get; set; }

		public string SelectionName { get; set; }

		public decimal Coefficient { get; set; }

		public string RoundName { get; set; }

		public string UnitName { get; set; }

		public string MarketName { get; set; }

		public DateTime? EventDate { get; set; }
	}
}