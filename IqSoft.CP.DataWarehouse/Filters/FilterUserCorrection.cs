using IqSoft.CP.Common.Models;
using System;
using System.Linq;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterUserCorrection : FilterBase<fnReportByUserCorrection>
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation TotalDebits { get; set; }
        public FiltersOperation TotalCredits { get; set; }
        public FiltersOperation Balances { get; set; }

        public override void CreateQuery(ref IQueryable<fnReportByUserCorrection> objects, bool orderBy, bool orderByDate = false)
        {
            if(PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, TotalDebits, "TotalDebit");
            FilterByValue(ref objects, TotalCredits, "TotalCredit");
            FilterByValue(ref objects, Balances, "Balance");
            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }

        public IQueryable<fnReportByUserCorrection> FilterObjects(IQueryable<fnReportByUserCorrection> userCorrection, bool ordering)
        {
            CreateQuery(ref userCorrection, ordering);
            return userCorrection;
        }

        public long SelectedObjectsCount(IQueryable<fnReportByUserCorrection> userCorrection)
        {
            CreateQuery(ref userCorrection, false);
            return userCorrection.Count();
        }
    }
}