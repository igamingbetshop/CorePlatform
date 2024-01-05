﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class FinOperationResponse : ClientRequestResponseBase
	{
		public decimal CashierBalance { get; set; }
		public decimal Balance
		{
			get { return Math.Floor(CashierBalance * 100) / 100; }
		}

		private decimal _clientBalance;
		public decimal ClientBalance
		{
			get { return Math.Floor(_clientBalance * 100) / 100; }
			set { _clientBalance = value; }
		}

		public decimal CurrentLimit { get; set; }
		public string CurrencyId { get; set; }
	}
}