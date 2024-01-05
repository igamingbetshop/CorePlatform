using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterBetShopGroup : FilterBase<BetShopGroup>
    {
        public int? Id { get; set; }

        public int? ParentId { get; set; }

        public int? PartnerId { get; set; }

        public string Path { get; set; }

        public bool? IsRoot { get; set; }

        public bool? IsLeaf { get; set; }

        public string Name { get; set; }

        public int? State { get; set; }

        protected override IQueryable<BetShopGroup> CreateQuery(IQueryable<BetShopGroup> objects, Func<IQueryable<BetShopGroup>, IOrderedQueryable<BetShopGroup>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId.Value);
            if (IsRoot.HasValue)
                if (IsRoot.Value)
                    objects = objects.Where(x => x.ParentId == null);
                else
                    objects = objects.Where(x => x.ParentId != null);
            if (IsLeaf.HasValue)
                if (IsLeaf.Value)
                    objects = objects.Where(x => x.IsLeaf);
                else
                    objects = objects.Where(x => !x.IsLeaf);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name == Name);
            if (!string.IsNullOrWhiteSpace(Path))
                objects = objects.Where(x => x.Path.StartsWith(Path));
            if (State.HasValue)
                objects = objects.Where(x => x.State == State.Value);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<BetShopGroup> FilterObjects(IQueryable<BetShopGroup> betShopGroups, Func<IQueryable<BetShopGroup>, IOrderedQueryable<BetShopGroup>> orderBy = null)
        {
            betShopGroups = CreateQuery(betShopGroups, orderBy);
            return betShopGroups;
        }

        public long SelectedObjectsCount(IQueryable<BetShopGroup> betShopGroups)
        {
            betShopGroups = CreateQuery(betShopGroups);
            return betShopGroups.Count();
        }
    }
}
