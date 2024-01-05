using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime BetDate { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime? WinDate { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime? PayDate { get; set; }

		public bool IsLive { get; set; }

		public List<BllBetSelection> BetSelections { get; set; }
	}
}