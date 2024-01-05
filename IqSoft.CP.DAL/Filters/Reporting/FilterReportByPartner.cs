using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByPartner : FilterBase<fnReportByPartner>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation PartnerNames { get; set; }
        public FiltersOperation TotalBetAmounts { get; set; }
        public FiltersOperation TotalBetsCounts { get; set; }
        public FiltersOperation TotalWinAmounts { get; set; }
        public FiltersOperation TotalGGRs { get; set; }

        protected override IQueryable<fnReportByPartner> CreateQuery(IQueryable<fnReportByPartner> objects, Func<IQueryable<fnReportByPartner>, IOrderedQueryable<fnReportByPartner>> orderBy = null)
        {

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, PartnerNames, "PartnerName");
            FilterByValue(ref objects, TotalBetAmounts, "TotalBetAmount");
            FilterByValue(ref objects, TotalWinAmounts, "TotalWinAmount");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, TotalGGRs, "TotalGGR");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByPartner> FilterObjects(IQueryable<fnReportByPartner> objects, Func<IQueryable<fnReportByPartner>, IOrderedQueryable<fnReportByPartner>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
    }
}
