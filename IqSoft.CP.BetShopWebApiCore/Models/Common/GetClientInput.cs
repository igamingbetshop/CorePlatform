using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetClientInput : PlatformRequestBase
	{
		public int? ClientId { get; set; }

		public int CashDeskId { get; set; }

		public string UserName { get; set; }

		public string DocumentNumber { get; set; }

		public string Email { get; set; }
	}
}