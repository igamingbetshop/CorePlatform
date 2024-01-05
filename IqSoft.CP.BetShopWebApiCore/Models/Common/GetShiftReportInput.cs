using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetShiftReportInput : PlatformRequestBase
	{
		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public int? CashierId { get; set; }

		public int CashDeskId { get; set; }
	}
}