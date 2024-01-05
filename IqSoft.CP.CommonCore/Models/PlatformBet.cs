using System;

namespace IqSoft.CP.Common.Models
{
	public class PlatformBet
	{
		public long Id { get; set; }
		public long BetDocumentId { get; set; }
		public long? WinDocumentId { get; set; }
		public long? PayDocumentId { get; set; }
		public string CurrencyId { get; set; }
		public int ProductId { get; set; }
		public decimal BetAmount { get; set; }
		public decimal WinAmount { get; set; }
		public int State { get; set; }
		public int TypeId { get; set; }
		public int? CashDeskId { get; set; }
		public int? ClientId { get; set; }
		public long? TicketNumber { get; set; }
		public int? UserId { get; set; }
		public int DeviceTypeId { get; set; }
		public DateTime BetTime { get; set; }
		public DateTime? CalculationTime { get; set; }
		public DateTime? PayTime { get; set; }
		public long BetDate { get; set; }
		public long? CalculationDate { get; set; }
		public long? PayDate { get; set; }
		public int? BetShopId { get; set; }
	}
}
