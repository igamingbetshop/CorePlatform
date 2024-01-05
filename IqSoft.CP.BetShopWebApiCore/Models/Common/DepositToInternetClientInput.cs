using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class DepositToInternetClientInput : PlatformRequestBase
	{
		public int ClientId { get; set; }

		public string TransactionId { get; set; }

		public int CashierId { get; set; }

		public int CashDeskId { get; set; }

		public decimal Amount { get; set; }

		public string CurrencyId { get; set; }
	}
}