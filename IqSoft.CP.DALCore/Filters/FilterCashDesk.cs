using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterCashDesk : FilterBase<CashDesk>
    {
        public int? Id { get; set; }

        public int? BetShopId { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<CashDesk> CreateQuery(IQueryable<CashDesk> objects, Func<IQueryable<CashDesk>, IOrderedQueryable<CashDesk>> orderBy = null)
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

        public IQueryable<CashDesk> FilterObjects(IQueryable<CashDesk> cashDesks, Func<IQueryable<CashDesk>, IOrderedQueryable<CashDesk>> orderBy = null)
        {
            cashDesks = CreateQuery(cashDesks, orderBy);
            return cashDesks;
        }

        public long SelectedObjectsCount(IQueryable<CashDesk> cashDesks)
        {
            cashDesks = CreateQuery(cashDesks);
            return cashDesks.Count();
        }
    }
}
