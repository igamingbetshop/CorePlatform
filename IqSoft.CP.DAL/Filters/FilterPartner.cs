using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPartner : FilterBase<Partner>
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string CurrencyId { get; set; }

        public int? State { get; set; }

        public string AdminSiteUrl { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<Partner> CreateQuery(IQueryable<Partner> objects, Func<IQueryable<Partner>, IOrderedQueryable<Partner>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name.Contains(Name));
            if (!string.IsNullOrWhiteSpace(CurrencyId))
                objects = objects.Where(x => x.CurrencyId == CurrencyId);
            if (State.HasValue)
                objects = objects.Where(x => x.State == State.Value);
            if (!string.IsNullOrWhiteSpace(AdminSiteUrl))
                objects = objects.Where(x => x.AdminSiteUrl.Contains(AdminSiteUrl));
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Partner> FilterObjects(IQueryable<Partner> partners, Func<IQueryable<Partner>, IOrderedQueryable<Partner>> orderBy = null)
        {
            partners = CreateQuery(partners, orderBy);
            return partners;
        }

        public long SelectedObjectsCount(IQueryable<Partner> partners)
        {
            partners = CreateQuery(partners);
            return partners.Count();
        }
    }
}
