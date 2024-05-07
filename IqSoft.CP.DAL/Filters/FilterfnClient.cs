using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnClient : FilterBase<fnClient>
    {
        public int? PartnerId { get; set; }

        public int? AgentId { get; set; }
        
        public int? AffiliateId { get; set; }

        public string RefId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
        public string UnderMonitoringTypes { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation Emails { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation LanguageIds { get; set; }

        public FiltersOperation Categories { get; set; }

        public FiltersOperation Genders { get; set; }

        public FiltersOperation FirstNames { get; set; }

        public FiltersOperation LastNames { get; set; }

        public FiltersOperation NickNames { get; set; }
        
        public FiltersOperation SecondNames { get; set; }
        
        public FiltersOperation SecondSurnames { get; set; }

        public FiltersOperation DocumentNumbers { get; set; }

        public FiltersOperation DocumentIssuedBys { get; set; }

        public FiltersOperation MobileNumbers { get; set; }

        public FiltersOperation ZipCodes { get; set; }
        
        public FiltersOperation Cities { get; set; }

        public FiltersOperation IsDocumentVerifieds { get; set; }

        public FiltersOperation PhoneNumbers { get; set; }

        public FiltersOperation RegionIds { get; set; }
        public FiltersOperation CountryIds { get; set; }

        public FiltersOperation BirthDates { get; set; }
        public FiltersOperation Ages { get; set; }
        public FiltersOperation RegionIsoCodes { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation CreationTimes { get; set; }

        public FiltersOperation LastUpdateTimes { get; set; }
        public FiltersOperation LastSessionDates { get; set; }

        public FiltersOperation RealBalances { get; set; }
        public FiltersOperation BonusBalances { get; set; }

        public FiltersOperation GGRs { get; set; }

        public FiltersOperation NETGamings { get; set; }

        public FiltersOperation AffiliatePlatformIds { get; set; }

        public FiltersOperation AffiliateIds { get; set; }
        
        public FiltersOperation AffiliateReferralIds { get; set; }

        public FiltersOperation UserIds { get; set; }

        public FiltersOperation LastDepositDates { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        protected override IQueryable<fnClient> CreateQuery(IQueryable<fnClient> objects, Func<IQueryable<fnClient>, IOrderedQueryable<fnClient>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (AgentId.HasValue)
            {
                var path = "/" + AgentId.Value + "/";
                objects = objects.Where(x => x.UserPath.Contains(path));
            }
            if (AffiliateId.HasValue)
                objects = objects.Where(x => x.AffiliateId == AffiliateId.Value.ToString() && x.AffiliatePlatformId == PartnerId * 100);
            if (!string.IsNullOrEmpty(RefId))
                objects = objects.Where(x => x.AffiliateReferralId == RefId);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            if (!string.IsNullOrEmpty(UnderMonitoringTypes))
                objects = objects.Where(x => x.UnderMonitoringTypes.Contains(UnderMonitoringTypes ));


            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, LanguageIds, "LanguageId");
            FilterByValue(ref objects, Genders, "Gender");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, SecondNames, "SecondName");
            FilterByValue(ref objects, SecondSurnames, "SecondSurname");
            FilterByValue(ref objects, DocumentNumbers, "DocumentNumber");
            FilterByValue(ref objects, DocumentIssuedBys, "DocumentIssuedBy");
            FilterByValue(ref objects, MobileNumbers, "MobileNumber");
            FilterByValue(ref objects, ZipCodes, "ZipCode");
            FilterByValue(ref objects, Cities, "City");
            FilterByValue(ref objects, IsDocumentVerifieds, "IsDocumentVerified");
            FilterByValue(ref objects, PhoneNumbers, "PhoneNumber");
            FilterByValue(ref objects, RegionIds, "RegionId");
            FilterByValue(ref objects, CountryIds, "CountryId");
            FilterByValue(ref objects, BirthDates, "BirthDate");
            FilterByValue(ref objects, Ages, "Age");
            FilterByValue(ref objects, RegionIsoCodes, "RegionIsoCode");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, RealBalances, "RealBalance");
            FilterByValue(ref objects, BonusBalances, "BonusBalance");
            FilterByValue(ref objects, GGRs, "GGR");
            FilterByValue(ref objects, NETGamings, "NETGaming");
            FilterByValue(ref objects, AffiliatePlatformIds, "AffiliatePlatformId");
            FilterByValue(ref objects, AffiliateIds, "AffiliateId");
            FilterByValue(ref objects, AffiliateReferralIds, "AffiliateReferralId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, LastDepositDates, "LastDepositDate");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            FilterByValue(ref objects, LastSessionDates, "LastSessionDate");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnClient> FilterObjects(IQueryable<fnClient> clients, Func<IQueryable<fnClient>, IOrderedQueryable<fnClient>> orderBy = null)
        {
            clients = CreateQuery(clients, orderBy);
            return clients;
        }

        public long SelectedObjectsCount(IQueryable<fnClient> clients)
        {
            clients = CreateQuery(clients);
            return clients.Count();
        }
    }
}
