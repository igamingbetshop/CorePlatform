using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByProduct : FilterBase<fnReportByProduct>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation ClientNames { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation ProductIds { get; set; }

        public FiltersOperation ProductNames { get; set; }

        public FiltersOperation DeviceTypeIds { get; set; }

        public FiltersOperation ProviderNames { get; set; }

        public FiltersOperation TotalBetsAmounts { get; set; }

        public FiltersOperation TotalWinsAmounts { get; set; }

        public FiltersOperation TotalBetsCounts { get; set; }

        public FiltersOperation TotalUncalculatedBetsCounts { get; set; }

        public FiltersOperation TotalUncalculatedBetsAmounts { get; set; }

        public FiltersOperation GGRs { get; set; }

        protected override IQueryable<fnReportByProduct> CreateQuery(IQueryable<fnReportByProduct> objects, Func<IQueryable<fnReportByProduct>, IOrderedQueryable<fnReportByProduct>> orderBy = null)
        {
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, Currencies, "Currency");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, DeviceTypeIds, "DeviceTypeId");
            FilterByValue(ref objects, ProviderNames, "ProviderName");
            FilterByValue(ref objects, TotalBetsAmounts, "TotalBetsAmount");
            FilterByValue(ref objects, TotalWinsAmounts, "TotalWinsAmount");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, TotalUncalculatedBetsCounts, "TotalUncalculatedBetsCount");
            FilterByValue(ref objects, TotalUncalculatedBetsAmounts, "TotalUncalculatedBetsAmount");
            FilterByValue(ref objects, GGRs, "GGR");
            FilterByValue(ref objects, ClientNames, "ClientFirstName", "ClientLastName");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByProduct> FilterObjects(IQueryable<fnReportByProduct> objects, Func<IQueryable<fnReportByProduct>, IOrderedQueryable<fnReportByProduct>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
    }
}