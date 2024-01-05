using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskCurrentBalanceOutput : ClientRequestResponseBase
	{
		public decimal Balance { get; set; }

		public decimal CurrentLimit { get; set; }
	}
}