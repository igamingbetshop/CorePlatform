using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetOperationsInput : PlatformRequestBase
	{
		public int CashierId { get; set; }

		public int CashDeskId { get; set; }

		public DateTime FromDate { get; set; }

		public DateTime ToDate { get; set; }
	}
}