using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetShopBetsInput : PlatformRequestBase
	{
		public int CashierId { get; set; }
		public int CashDeskId { get; set; }
		public int? ProductId { get; set; }
		public long? Barcode { get; set; }
		public int? State { get; set; }
		public DateTime? BetDateFrom { get; set; }
		public DateTime? BetDateBefore { get; set; }
	}
}