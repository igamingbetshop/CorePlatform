using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDesksBalanceOutput : ClientRequestResponseBase
	{
		public List<CashDeskBalanceOutput> CashDeskBalances { get; set; }
	}

	public class CashDeskBalanceOutput
	{
		public int CashDeskId { get; set; }
		public decimal Balance { get; set; }
	}
}