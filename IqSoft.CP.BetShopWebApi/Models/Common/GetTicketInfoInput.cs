using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetTicketInfoInput : PlatformRequestBase
	{
		public string Code { get; set; }
		public string TicketId { get; set; }
		public int CashDeskId { get; set; }
		public string ProductToken { get; set; }
	}
}