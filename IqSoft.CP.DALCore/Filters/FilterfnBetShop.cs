using System;
using System.Linq;

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
