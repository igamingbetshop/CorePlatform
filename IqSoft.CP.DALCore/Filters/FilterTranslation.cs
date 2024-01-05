using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterTranslation : FilterBase<Translation>
    {
        public long? ObjectId { get; set; }

        public int? ObjectTypeId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<Translation> CreateQuery(IQueryable<Translation> objects, Func<IQueryable<Translation>, IOrderedQueryable<Translation>> orderBy = null)
        {
            if (ObjectId.HasValue)
                objects = objects.Where(x => x.Id == ObjectId.Value);
            if (ObjectTypeId.HasValue)
                objects = objects.Where(x => x.ObjectTypeId == ObjectTypeId.Value);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Translation> FilterObjects(IQueryable<Translation> objects, Func<IQueryable<Translation>, IOrderedQueryable<Translation>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }

        public long SelectedObjectsCount(IQueryable<Translation> translations)
        {
            translations = CreateQuery(translations);
            return translations.Count();
        }
    }
}
