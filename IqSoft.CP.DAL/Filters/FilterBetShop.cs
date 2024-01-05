using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterBetShop : FilterBase<BetShop>
    {
        public int? Id { get; set; }

        public int? GroupId { get; set; }

        public string CurrencyId { get; set; }

        public string Address { get; set; }

        public int? PartnerId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<BetShop> CreateQuery(IQueryable<BetShop> objects, Func<IQueryable<BetShop>, IOrderedQueryable<BetShop>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (GroupId.HasValue)
                objects = objects.Where(x => x.GroupId == GroupId.Value);
            if (!string.IsNullOrWhiteSpace(CurrencyId))
                objects = objects.Where(x => x.CurrencyId == CurrencyId);
            if (!string.IsNullOrWhiteSpace(Address))
                objects = objects.Where(x => x.Address.Contains(Address));
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<BetShop> FilterObjects(IQueryable<BetShop> betshops, Func<IQueryable<BetShop>, IOrderedQueryable<BetShop>> orderBy = null)
        {
            betshops = CreateQuery(betshops, orderBy);
            return betshops;
        }

        public long SelectedObjectsCount(IQueryable<BetShop> betshops)
        {
            betshops = CreateQuery(betshops);
            return betshops.Count();
        }
    }
}
 