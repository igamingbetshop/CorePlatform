using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnAccount : FilterBase<fnAccount>
    {
        public int? Id { get; set; }

        public int? ObjectId { get; set; }

        public int? ObjectTypeId { get; set; }

        public int? AccountTypeId { get; set; }

        public string CurrencyId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<fnAccount> CreateQuery(IQueryable<fnAccount> objects, Func<IQueryable<fnAccount>, IOrderedQueryable<fnAccount>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (ObjectId.HasValue)
                objects = objects.Where(x => x.ObjectId == ObjectId.Value);
            if (ObjectTypeId.HasValue)
                objects = objects.Where(x => x.ObjectTypeId == ObjectTypeId.Value);
            if (AccountTypeId.HasValue)
                objects = objects.Where(x => x.TypeId == AccountTypeId.Value);
            if (!string.IsNullOrWhiteSpace(CurrencyId))
                objects = objects.Where(x => x.CurrencyId == CurrencyId);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            return base.FilteredObjects(objects);
        }

        public IQueryable<fnAccount> FilterObjects(IQueryable<fnAccount> accounts)
        {
            accounts = CreateQuery(accounts);
            return accounts;
        }

        public long SelectedObjectsCount(IQueryable<fnAccount> accounts)
        {
            accounts = CreateQuery(accounts);
            return accounts.Count();
        }
    }
}
