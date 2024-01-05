using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskInfoOutput : ClientRequestResponseBase
	{
		public int CashDeskId { get; set; }

		private decimal _balance;
		public decimal Balance
		{
			get { return Math.Floor(_balance * 100) / 100; }
			set { _balance = value; }
		}

		public string CurrencyId { get; set; }

		public decimal CurrentLimit { get; set; }
	}
}