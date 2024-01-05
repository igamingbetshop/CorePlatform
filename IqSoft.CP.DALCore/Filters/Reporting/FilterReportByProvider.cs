using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByProvider : FilterBase<fnReportByProvider>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation ProviderNames { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation TotalBetsCounts { get; set; }

        public FiltersOperation TotalBetsAmounts { get; set; }

        public FiltersOperation TotalWinsAmounts { get; set; }

        public FiltersOperation TotalUncalculatedBetsCounts { get; set; }

        public FiltersOperation TotalUncalculatedBetsAmounts { get; set; }

        public FiltersOperation GGRs { get; set; }

        protected override IQueryable<fnReportByProvider> CreateQuery(IQueryable<fnReportByProvider> objects, Func<IQueryable<fnReportByProvider>, IOrderedQueryable<fnReportByProvider>> orderBy = null)
        {
            FilterByValue(ref objects, ProviderNames, "ProviderName");
            FilterByValue(ref objects, Currencies, "Currency");
            FilterByValue(ref objects, TotalBetsAmounts, "TotalBetsAmount");
            FilterByValue(ref objects, TotalWinsAmounts, "TotalWinsAmount");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, TotalUncalculatedBetsCounts, "TotalUncalculatedBetsCount");
            FilterByValue(ref objects, TotalUncalculatedBetsAmounts, "TotalUncalculatedBetsAmount");
            FilterByValue(ref objects, GGRs, "GGR");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByProvider> FilterObjects(IQueryable<fnReportByProvider> objects, Func<IQueryable<fnReportByProvider>, IOrderedQueryable<fnReportByProvider>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
    }
}