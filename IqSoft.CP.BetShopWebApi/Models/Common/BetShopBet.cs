using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class BetShopBet
	{
		public long BetDocumentId { get; set; }

		public int State { get; set; }

		public int? CashDeskId { get; set; }

		public decimal BetAmount { get; set; }

		public decimal WinAmount { get; set; }

		public int ProductId { get; set; }

		public string ProductName { get; set; }

		public int? GameProviderId { get; set; }

		public long? Barcode { get; set; }

		public long? TicketNumber { get; set; }

		public int? CashierId { get; set; }

		public int BetType { get; set; }

		public decimal Coefficient { get; set; }

		public DateTime BetDate { get; set; }

		public DateTime? WinDate { get; set; }

		public DateTime? PayDate { get; set; }

		public bool IsLive { get; set; }

		public List<BllBetSelection> BetSelections { get; set; }
		public int NumberOfBets { get; set; }
		public int NumberOfMatches { get; set; }
		public decimal AmountPerBet { get; set; }
		public decimal CommissionFee { get; set; }
		public decimal PossibleWin { get; set; }
		public List<int> SystemOutCounts { get; set; }
		public decimal CashoutAmount { get; set; }

		public bool BlockedForCashout { get; set; }
	}
}