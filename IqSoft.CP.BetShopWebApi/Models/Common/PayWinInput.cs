using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PayWinInput : PlatformRequestBase
	{
		public int CashDeskId { get; set; }

		public int CashierId { get; set; }

		public long BetDocumentId { get; set; }

		public string ExternalTransactionId { get; set; }
	}
}