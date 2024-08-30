using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
	public class ApiFilterClientGame : ApiFilterBase
	{
		public int? PartnerId { get; set; }

		public DateTime FromDate { get; set; }

		public DateTime ToDate { get; set; }

		public ApiFiltersOperation ClientIds { get; set; }

		public ApiFiltersOperation FirstNames { get; set; }

		public ApiFiltersOperation LastNames { get; set; }

		public ApiFiltersOperation ProductIds { get; set; }

		public ApiFiltersOperation ProductNames { get; set; }

		public ApiFiltersOperation ProviderNames { get; set; }

		public ApiFiltersOperation CurrencyIds { get; set; }
	}
}