using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class CancelBetSelectionInput
	{
		public string GameId { get; set; }

		public string Token { get; set; }

		public List<CancelBetSelectionInputItem> Selections { get; set; }
	}

	public class CancelBetSelectionInputItem
	{
		public int GameUnitId { get; set; }

		public int SelectionTypeId { get; set; }

		public long SelectionId { get; set; }
	}
}