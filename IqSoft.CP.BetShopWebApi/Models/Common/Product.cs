using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class Product
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string CashierUrl { get; set; }

		public string MonitorUrl { get; set; }

		public List<int> InterfaceTypes { get; set; }
	}
}