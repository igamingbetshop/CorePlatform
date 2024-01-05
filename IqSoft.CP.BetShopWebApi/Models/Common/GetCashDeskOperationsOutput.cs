using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskOperationsOutput : ClientRequestResponseBase
	{
		public List<CashDeskOperation> Operations { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime EndTime { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime StartTime { get; set; }
	}

	public class CashDeskOperation
	{
		public long Id { get; set; }

		public string ExternalTransactionId { get; set; }

		public decimal Amount { get; set; }

		public string CurrencyId { get; set; }

		public int State { get; set; }

		public string Info { get; set; }

		public int? Creator { get; set; }

		public int? CashDeskId { get; set; }

		public long? TicketNumber { get; set; }

		public string TicketInfo { get; set; }

		public int? CashierId { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime CreationTime { get; set; }

		public string OperationTypeName { get; set; }

		public string CashDeskName { get; set; }

		public string BetShopName { get; set; }

		public int BetShopId { get; set; }
		public int? ClientId { get; set; }
		public long? PaymentRequestId { get; set; }
	}
}