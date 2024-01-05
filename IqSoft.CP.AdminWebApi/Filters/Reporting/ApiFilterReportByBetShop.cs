using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
	public class ApiFilterReportByBetShop
	{
		public int? PartnerId { get; set; }

		public DateTime BetDateFrom { get; set; }

		public DateTime BetDateBefore { get; set; }

		public ApiFiltersOperation ProductIds { get; set; }

		public ApiFiltersOperation BetShopIds { get; set; }

		public ApiFiltersOperation BetShopGroupIds { get; set; }

		public ApiFiltersOperation BetShopNames { get; set; }
		
		public ApiFiltersOperation Currencies { get; set; }
	}
}