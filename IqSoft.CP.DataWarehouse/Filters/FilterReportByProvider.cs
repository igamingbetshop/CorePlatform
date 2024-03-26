using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
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

        public override void CreateQuery(ref IQueryable<fnReportByProvider> objects, bool order, bool orderByDate = false)
        {
            FilterByValue(ref objects, ProviderNames, "ProviderName");
            FilterByValue(ref objects, Currencies, "Currency");
            FilterByValue(ref objects, TotalBetsAmounts, "TotalBetsAmount");
            FilterByValue(ref objects, TotalWinsAmounts, "TotalWinsAmount");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, TotalUncalculatedBetsCounts, "TotalUncalculatedBetsCount");
            FilterByValue(ref objects, TotalUncalculatedBetsAmounts, "TotalUncalculatedBetsAmount");
            FilterByValue(ref objects, GGRs, "GGR");

            base.FilteredObjects(ref objects, order, orderByDate, null);
        }

        public IQueryable<fnReportByProvider> FilterObjects(IQueryable<fnReportByProvider> objects, bool order)
        {
            CreateQuery(ref objects, order);
            return objects;
        }
    }
}