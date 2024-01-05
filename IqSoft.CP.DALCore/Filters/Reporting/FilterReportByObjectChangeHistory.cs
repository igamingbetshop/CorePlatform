using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByObjectChangeHistory : FilterBase<spObjectChangeHistory>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ObjectId { get; set; }
        public int? PartnerId { get; set; }
        public int ObjectTypeId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ObjectIds { get; set; }
        public FiltersOperation UserIds { get; set; }

        protected override IQueryable<spObjectChangeHistory> CreateQuery(IQueryable<spObjectChangeHistory> objects, Func<IQueryable<spObjectChangeHistory>, IOrderedQueryable<spObjectChangeHistory>> orderBy = null)
        {
            objects = objects.Where(x => x.ChangeDate >= FromDate);
            objects = objects.Where(x => x.ChangeDate < ToDate);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (ObjectId.HasValue)
                objects = objects.Where(x => x.ObjectId == ObjectId.Value);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ObjectIds, "ObjectId");
            FilterByValue(ref objects, UserIds, "UserId");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<spObjectChangeHistory> FilterObjects(IQueryable<spObjectChangeHistory> objects, Func<IQueryable<spObjectChangeHistory>, IOrderedQueryable<spObjectChangeHistory>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }

        public long SelectedObjectsCount(IQueryable<spObjectChangeHistory> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }
    }
}
