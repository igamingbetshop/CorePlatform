using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnRegion: FilterBase<fnRegion>
    {
        public int? Id { get; set; }

        public int? ParentId { get; set; }

        public int? TypeId { get; set; }

        public int? State { get; set; }

        public string Name { get; set; }

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
            if (!string.IsNullOrEmpty(Name))
                objects = objects.Where(x => x.Name.Contains(Name));

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnRegion> FilterObjects(IQueryable<fnRegion> fnRegions, Func<IQueryable<fnRegion>, IOrderedQueryable<fnRegion>> orderBy = null)
        {
            fnRegions = CreateQuery(fnRegions, orderBy);
            return fnRegions;
        }


        public long SelectedObjectsCount(IQueryable<fnRegion> fnRegions)
        {
            fnRegions = CreateQuery(fnRegions);
            return fnRegions.Count();
        }
    }
}
