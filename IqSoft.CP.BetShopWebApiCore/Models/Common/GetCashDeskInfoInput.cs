﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskInfoInput : PlatformRequestBase
	{
		public int CashDeskId { get; set; }
		public int ProductId { get; set; }
	}
}