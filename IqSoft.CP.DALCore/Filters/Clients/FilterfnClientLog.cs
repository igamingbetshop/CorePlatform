using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Clients
{
    public class FilterfnClientLog : FilterBase<fnClientLog>
    {
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation Actions { get; set; }

        public FiltersOperation UserIds { get; set; }

        public FiltersOperation Ips { get; set; }

        public FiltersOperation Pages { get; set; }

        public FiltersOperation SessionIds { get; set; }

        protected override IQueryable<fnClientLog> CreateQuery(IQueryable<fnClientLog> objects, Func<IQueryable<fnClientLog>, IOrderedQueryable<fnClientLog>> orderBy = null)
        {
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, Actions, "Action");
            FilterByValue(ref objects, SessionIds, "ClientSessionId");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnClientLog> FilterObjects(IQueryable<fnClientLog> objects, Func<IQueryable<fnClientLog>, IOrderedQueryable<fnClientLog>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }

        public long SelectedObjectsCount(IQueryable<fnClientLog> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }
    }
}
