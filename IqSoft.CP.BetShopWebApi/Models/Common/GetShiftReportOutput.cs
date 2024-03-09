using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetShiftReportOutput : ApiResponseBase
	{
		public List<ShiftReport> Shifts { get; set; }
	}

	public class ShiftReport
	{
		public long Id { get; set; }

		public string CashierFirstName { get; set; }

		public string CashierLastName { get; set; }

		public int BetShopId { get; set; }

		public int CashDeskId { get; set; }

		public string BetShopAddress { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime StartTime { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime? EndTime { get; set; }

		public decimal StartAmount { get; set; }

		public decimal BetAmounts { get; set; }

		public decimal PayedWins { get; set; }

		public decimal DepositToInternetClients { get; set; }

		public decimal WithdrawFromInternetClients { get; set; }

		public decimal DebitCorrectionOnCashDesk { get; set; }

		public decimal CreditCorrectionOnCashDesk { get; set; }

		public decimal Balance { get; set; }

		public decimal? EndAmount { get; set; }

		public decimal BonusAmount { get; set; }
	}
}