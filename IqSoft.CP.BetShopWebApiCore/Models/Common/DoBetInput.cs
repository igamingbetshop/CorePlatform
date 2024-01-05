using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class DoBetInput
	{
		public int BetType { get; set; }

		public string GameId { get; set; }

		public int ClientId { get; set; }

		public int CashDeskId { get; set; }

		public string Token { get; set; }

		public decimal Amount { get; set; }

		public int AcceptType { get; set; }

		public string Info { get; set; }

		public int? SystemOutCount { get; set; }

		public List<DoBetInputItem> Events { get; set; }
	}

	public class DoBetInputItem
	{
		public int UnitId { get; set; }

		public int RoundId { get; set; }

		public int MarketTypeId { get; set; }

		public long SelectionTypeId { get; set; }

		public long MarketId { get; set; }

		public long SelectionId { get; set; }

		public string SelectionName { get; set; }

		public decimal Coefficient { get; set; }

		public string RoundName { get; set; }

		public string UnitName { get; set; }

		public string MarketName { get; set; }

		public DateTime EventDate { get; set; }
	}
}