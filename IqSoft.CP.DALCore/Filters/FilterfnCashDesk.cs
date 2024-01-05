using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnCashDesk : FilterBase<fnCashDesks>
    {
        public int? Id { get; set; }

        public int? BetShopId { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<fnCashDesks> CreateQuery(IQueryable<fnCashDesks> objects, Func<IQueryable<fnCashDesks>, IOrderedQueryable<fnCashDesks>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (BetShopId.HasValue)
                objects = objects.Where(x => x.BetShopId == BetShopId.Value);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name.Contains(Name));
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnCashDesks> FilterObjects(IQueryable<fnCashDesks> cashDesks, Func<IQueryable<fnCashDesks>, IOrderedQueryable<fnCashDesks>> orderBy = null)
        {
            cashDesks = CreateQuery(cashDesks, orderBy);
            return cashDesks;
        }

        public long SelectedObjectsCount(IQueryable<fnCashDesks> cashDesks)
        {
            cashDesks = CreateQuery(cashDesks);
            return cashDesks.Count();
        }
    }
}
