using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PayPaymentRequestInput : PlatformRequestBase
	{
		public int CashDeskId { get; set; }
		public int CashierId { get; set; }
		public int PaymentRequestId { get; set; }
		public string Comment { get; set; }
	}
}