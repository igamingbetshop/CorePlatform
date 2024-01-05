using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetByBarcodeInput : PlatformRequestBase
	{
		public long Barcode { get; set; }

		public int CashDeskId { get; set; }
	}
}