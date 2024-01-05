using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels.Bets
{
	public class ApiGetBetInfoOutput
	{
		public string Url { get; set; }
		public string TransactionId { get; set; }

		public long Barcode { get; set; }

		public string TicketNumber { get; set; }

		public int GameId { get; set; }

		public string GameName { get; set; }

		public decimal Amount { get; set; }

		public DateTime BetDate { get; set; }

		public decimal Coefficient { get; set; }

		public int? PlayerBonusId { get; set; }

        public decimal WinAmount { get; set; }

		public int Status { get; set; }

		public decimal PossibleWin { get; set; }

        public List<ApiBetItem> BetSelections { get; set; }
	}
}