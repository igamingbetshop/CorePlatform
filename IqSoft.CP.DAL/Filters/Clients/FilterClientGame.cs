using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Clients
{
	public class FilterClientGame : FilterBase<fnReportByClientGame>
	{
		public int? PartnerId { get; set; }
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }

		public FiltersOperation ClientIds { get; set; }

		public FiltersOperation FirstNames { get; set; }

		public FiltersOperation LastNames { get; set; }

		public FiltersOperation ProductIds { get; set; }

		public FiltersOperation ProductNames { get; set; }

		public FiltersOperation ProviderNames { get; set; }

		public FiltersOperation Currencies { get; set; }

		protected override IQueryable<fnReportByClientGame> CreateQuery(IQueryable<fnReportByClientGame> objects, Func<IQueryable<fnReportByClientGame>, IOrderedQueryable<fnReportByClientGame>> orderBy = null)
		{
			if (PartnerId.HasValue)
				objects = objects.Where(x => x.PartnerId == PartnerId.Value);
			FilterByValue(ref objects, ClientIds, "ClientId");
			FilterByValue(ref objects, FirstNames, "FirstName");
			FilterByValue(ref objects, LastNames, "LastName");
			FilterByValue(ref objects, ProductIds, "ProductId");
			FilterByValue(ref objects, ProductNames, "ProductName");
			FilterByValue(ref objects, ProviderNames, "ProviderName");
			FilterByValue(ref objects, Currencies, "CurrencyId");

			return base.FilteredObjects(objects, orderBy);
		}

		public IQueryable<fnReportByClientGame> FilterObjects(IQueryable<fnReportByClientGame> objects, Func<IQueryable<fnReportByClientGame>, IOrderedQueryable<fnReportByClientGame>> orderBy = null)
		{
			objects = CreateQuery(objects, orderBy);
			return objects;
		}

		public long SelectedObjectsCount(IQueryable<fnReportByClientGame> objects)
		{
			objects = CreateQuery(objects);
			return objects.Count();
		}
	}
}

