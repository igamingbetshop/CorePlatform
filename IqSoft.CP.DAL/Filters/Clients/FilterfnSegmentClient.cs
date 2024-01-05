using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Clients
{
    public class FilterfnSegmentClient : FilterBase<fnSegmentClient>
    {
        public int? PartnerId { get; set; }

        public int? SegmentId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation Emails { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation LanguageIds { get; set; }

        public FiltersOperation Categories { get; set; }

        public FiltersOperation Genders { get; set; }

        public FiltersOperation FirstNames { get; set; }

        public FiltersOperation LastNames { get; set; }
        public FiltersOperation SecondNames { get; set; }
        public FiltersOperation SecondSurnames { get; set; }

        public FiltersOperation DocumentNumbers { get; set; }

        public FiltersOperation DocumentIssuedBys { get; set; }

        public FiltersOperation MobileNumbers { get; set; }

        public FiltersOperation ZipCodes { get; set; }

        public FiltersOperation IsDocumentVerifieds { get; set; }

        public FiltersOperation PhoneNumbers { get; set; }

        public FiltersOperation RegionIds { get; set; }

        public FiltersOperation BirthDates { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation SegmentIds { get; set; }

        public FiltersOperation UserIds { get; set; }
        public FiltersOperation AffiliatePlatformIds { get; set; }
        public FiltersOperation AffiliateIds { get; set; }
        public FiltersOperation AffiliateReferralIds { get; set; }

        protected override IQueryable<fnSegmentClient> CreateQuery(IQueryable<fnSegmentClient> objects, Func<IQueryable<fnSegmentClient>, IOrderedQueryable<fnSegmentClient>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (SegmentId.HasValue)
                objects = objects.Where(x => x.SegmentId == SegmentId.Value);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, LanguageIds, "LanguageId");
            FilterByValue(ref objects, Genders, "Gender");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, SecondNames, "SecondName");
            FilterByValue(ref objects, SecondSurnames, "SecondSurname");
            FilterByValue(ref objects, DocumentNumbers, "DocumentNumber");
            FilterByValue(ref objects, DocumentIssuedBys, "DocumentIssuedBy");
            FilterByValue(ref objects, MobileNumbers, "MobileNumber");
            FilterByValue(ref objects, ZipCodes, "ZipCode");
            FilterByValue(ref objects, IsDocumentVerifieds, "IsDocumentVerified");
            FilterByValue(ref objects, PhoneNumbers, "PhoneNumber");
            FilterByValue(ref objects, RegionIds, "RegionId");
            FilterByValue(ref objects, BirthDates, "BirthDate");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, SegmentIds, "SegmentId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, AffiliatePlatformIds, "AffiliatePlatformId");
            FilterByValue(ref objects, AffiliateIds, "AffiliateId");
            FilterByValue(ref objects, AffiliateReferralIds, "AffiliateReferralId");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnSegmentClient> FilterObjects(IQueryable<fnSegmentClient> clients, Func<IQueryable<fnSegmentClient>, IOrderedQueryable<fnSegmentClient>> orderBy = null)
        {
            clients = CreateQuery(clients, orderBy);
            return clients;
        }

        public long SelectedObjectsCount(IQueryable<fnSegmentClient> clients)
        {
            clients = CreateQuery(clients);
            return clients.Count();
        }
    }
}
