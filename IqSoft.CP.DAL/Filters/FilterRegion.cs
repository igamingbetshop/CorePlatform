using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterRegion: FilterBase<fnRegion>
    {
        public int? Id { get; set; }

        public int? ParentId { get; set; }

        public int? TypeId { get; set; }

        public int? State { get; set; }

        protected override IQueryable<fnRegion> CreateQuery(IQueryable<fnRegion> objects, Func<IQueryable<fnRegion>, IOrderedQueryable<fnRegion>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id);
            if (ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId);
            if (TypeId.HasValue)
                objects = objects.Where(x => x.TypeId == TypeId);
            if (State.HasValue)
                objects = objects.Where(x => x.State == State);

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnRegion> FilterObjects(IQueryable<fnRegion> fnRegions, Func<IQueryable<fnRegion>, IOrderedQueryable<fnRegion>> orderBy = null)
        {
            fnRegions = CreateQuery(fnRegions, orderBy);
            return fnRegions;
        }

        public long SelectedObjectsCount(IQueryable<fnRegion> regions)
        {
            regions = CreateQuery(regions);
            return regions.Count();
        }
    }
}
