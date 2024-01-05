using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskBalanceIntput : PlatformRequestBase
	{
		public DateTime BalanceDate { get; set; }
		public List<CashDeskBalanceInput> CashDesks { get; set; }
	}

	public class CashDeskBalanceInput
	{
		public int CashDeskId { get; set; }
		public string CurrencyId { get; set; }
	}
}