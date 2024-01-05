using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetPaymentRequestsInput : PlatformRequestBase
	{
		public int? ClientId { get; set; }

		public string DocumentNumber { get; set; }

		public string DocumentIssuedBy { get; set; }

		public int CashDeskId { get; set; }

		public string CashCode { get; set; }
	}
}