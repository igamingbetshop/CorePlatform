using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnBetShop : FilterBase<fnBetShops>
    {
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public int? PartnerId { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation GroupIds { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation CurrencyIds { get; set; }

        public FiltersOperation Names { get; set; }

        public FiltersOperation Addresses { get; set; }

        public FiltersOperation Balances { get; set; }

        public FiltersOperation CurrentLimits { get; set; }

        public FiltersOperation AgentIds { get; set; }
        public FiltersOperation MaxCopyCounts { get; set; }
        public FiltersOperation MaxWinAmounts { get; set; }
        public FiltersOperation MinBetAmounts { get; set; }
        public FiltersOperation MaxEventCountPerTickets { get; set; }
        public FiltersOperation CommissionTypes { get; set; }
        public FiltersOperation CommissionRates { get; set; }
        public FiltersOperation AnonymousBets { get; set; }
        public FiltersOperation AllowCashouts { get; set; }
        public FiltersOperation AllowLives { get; set; }
        public FiltersOperation UsePins { get; set; }

        protected override IQueryable<fnBetShops> CreateQuery(IQueryable<fnBetShops> objects, Func<IQueryable<fnBetShops>, IOrderedQueryable<fnBetShops>> orderBy = null)
        {
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, GroupIds, "GroupId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, Names, "Name");
            FilterByValue(ref objects, CurrentLimits, "CurrentLimit");
            FilterByValue(ref objects, Balances, "Balance");
            FilterByValue(ref objects, Addresses, "Name");
            FilterByValue(ref objects, AgentIds, "AgentId");
            FilterByValue(ref objects, MaxCopyCounts, "MaxCopyCount");
            FilterByValue(ref objects, MaxWinAmounts, "MaxWinAmount");
            FilterByValue(ref objects, MinBetAmounts, "MinBetAmount");
            FilterByValue(ref objects, MaxEventCountPerTickets, "MaxEventCountPerTicket");
            FilterByValue(ref objects, CommissionTypes, "CommissionType");
            FilterByValue(ref objects, CommissionRates, "CommissionRate");
            FilterByValue(ref objects, AnonymousBets, "AnonymousBet");
            FilterByValue(ref objects, AllowCashouts, "AllowCashout");
            FilterByValue(ref objects, AllowLives, "AllowLive");
            FilterByValue(ref objects, UsePins, "UsePin");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnBetShops> FilterObjects(IQueryable<fnBetShops> betshops, Func<IQueryable<fnBetShops>, IOrderedQueryable<fnBetShops>> orderBy = null)
        {
            betshops = CreateQuery(betshops, orderBy);
            return betshops;
        }

        public long SelectedObjectsCount(IQueryable<fnBetShops> betshops)
        {
            betshops = CreateQuery(betshops);
            return betshops.Count();
        }
    }
}
