using IqSoft.CP.Common.Models;
using System;
using System.Linq;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterfnAffiliateCorrection : FilterBase<fnAffiliateCorrection>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? AffiliateId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation AffiliateIds { get; set; }
        public FiltersOperation FirstNames { get; set; }
        public FiltersOperation LastNames { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation Creators { get; set; }
        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }
        public FiltersOperation OperationTypeNames { get; set; }
        public FiltersOperation CreatorFirstNames { get; set; }
        public FiltersOperation CreatorLastNames { get; set; }
        public FiltersOperation DocumentTypeIds { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation ClientFirstNames { get; set; }
        public FiltersOperation ClientLastNames { get; set; }

        public override void CreateQuery(ref IQueryable<fnAffiliateCorrection> objects, bool orderBy, bool orderByDate = false)
        {
            if (AffiliateId.HasValue)
                objects = objects.Where(x => x.AffiliateId == AffiliateId.Value);
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, AffiliateIds, "AffiliateId");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, Creators, "Creator");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            FilterByValue(ref objects, OperationTypeNames, "OperationTypeName");
            FilterByValue(ref objects, CreatorFirstNames, "CreatorFirstName");
            FilterByValue(ref objects, CreatorLastNames, "CreatorLastName");
            FilterByValue(ref objects, DocumentTypeIds, "DocumentTypeId");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, ClientFirstNames, "ClientFirstName");
            FilterByValue(ref objects, ClientLastNames, "ClientLastName");
            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }

        public IQueryable<fnAffiliateCorrection> FilterObjects(IQueryable<fnAffiliateCorrection> affiliateCorrection, bool ordering)
        {
            CreateQuery(ref affiliateCorrection, ordering); 
            return affiliateCorrection;
        }

        public long SelectedObjectsCount(IQueryable<fnAffiliateCorrection> affiliateCorrection)
        {
            CreateQuery(ref affiliateCorrection, false);
            return affiliateCorrection.Count();
        }
    }
}
