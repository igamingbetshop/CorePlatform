using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models
{
	public class WebSocketHandlerInput
	{
		public string Token { get; set; }

		public int TimeZone { get; set; }

		public string LanguageId { get; set; }

		public int PartnerId { get; set; }

		public int CashDeskId { get; set; }

		public int BetShopId { get; set; }
	}
}