using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterNote : FilterBase<fnNote>
    {
        public long? Id { get; set; }

        public long? ObjectId { get; set; }

        public int? ObjectTypeId { get; set; }

        public string Message { get; set; }

        public int? Type { get; set; }

        public int? State { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        protected override IQueryable<fnNote> CreateQuery(IQueryable<fnNote> objects, Func<IQueryable<fnNote>, IOrderedQueryable<fnNote>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (ObjectId.HasValue)
                objects = objects.Where(x => x.ObjectId == ObjectId.Value);
            if (ObjectTypeId.HasValue)
                objects = objects.Where(x => x.ObjectTypeId == ObjectTypeId.Value);
            if (!string.IsNullOrWhiteSpace(Message))
                objects = objects.Where(x => x.Message.Contains(Message));
            if (State.HasValue)
                objects = objects.Where(x => x.State == State.Value);
			if (Type.HasValue)
				objects = objects.Where(x => x.Type == Type.Value);
			if (FromDate.HasValue)
                objects = objects.Where(x => x.CreationTime >= FromDate.Value);
            if (ToDate.HasValue)
                objects = objects.Where(x => x.CreationTime < ToDate.Value);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnNote> FilterObjects(IQueryable<fnNote> documents, Func<IQueryable<fnNote>, IOrderedQueryable<fnNote>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents;
        }

        public long SelectedObjectsCount(IQueryable<fnNote> documents, Func<IQueryable<fnNote>, IOrderedQueryable<fnNote>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents.Count();
        }
    }
}
