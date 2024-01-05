using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Affiliate
{
    public class FilterfnAffiliateClientInfo : FilterBase<fnAffiliateClientInfo>
    {
        public int? PartnerId { get; set; }
        public int? AffiliateId { get; set; }
        public string RefId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation CreationDates { get; set; }
        public FiltersOperation RefIds { get; set; }
        public FiltersOperation AffiliateIds { get; set; }
        public FiltersOperation AffiliateReferralIds { get; set; }
        public FiltersOperation ReferralIds { get; set; }
        public FiltersOperation FirstDepositDates { get; set; }
        public FiltersOperation LastDepositDates { get; set; }
        public FiltersOperation TotalDepositAmounts { get; set; }
        public FiltersOperation ConvertedTotalDepositAmounts { get; set; }

        protected override IQueryable<fnAffiliateClientInfo> CreateQuery(IQueryable<fnAffiliateClientInfo> objects, Func<IQueryable<fnAffiliateClientInfo>, IOrderedQueryable<fnAffiliateClientInfo>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
            if (AffiliateId.HasValue)
                objects = objects.Where(x => x.AffiliateId == AffiliateId.ToString());
            if (!string.IsNullOrEmpty(RefId))
                objects = objects.Where(x => x.AffiliateReferralId == RefId);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, CreationDates, "CreationDate");
            FilterByValue(ref objects, RefIds, "RefId");
            FilterByValue(ref objects, AffiliateIds, "AffiliateId");
            FilterByValue(ref objects, AffiliateReferralIds, "AffiliateReferralId");
            FilterByValue(ref objects, ReferralIds, "ReferralId");
            FilterByValue(ref objects, FirstDepositDates, "FirstDepositDate");
            FilterByValue(ref objects, LastDepositDates, "LastDepositDate");
            FilterByValue(ref objects, TotalDepositAmounts, "TotalDepositAmount");
            FilterByValue(ref objects, ConvertedTotalDepositAmounts, "ConvertedTotalDepositAmount");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAffiliateClientInfo> FilterObjects(IQueryable<fnAffiliateClientInfo> clients, Func<IQueryable<fnAffiliateClientInfo>, IOrderedQueryable<fnAffiliateClientInfo>> orderBy = null)
        {
            clients = CreateQuery(clients, orderBy);
            return clients;
        }

        public long SelectedObjectsCount(IQueryable<fnAffiliateClientInfo> clients)
        {
            clients = CreateQuery(clients);
            return clients.Count();
        }
    }
}