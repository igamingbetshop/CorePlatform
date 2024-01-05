using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Clients
{
    public class FilterReportByClientIdentity : FilterBase<fnClientIdentity>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool? HasNote { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation DocumentTypeIds { get; set; }
        public FiltersOperation Statuses { get; set; }
        public FiltersOperation ExpirationTimes { get; set; }
        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }

        protected override IQueryable<fnClientIdentity> CreateQuery(IQueryable<fnClientIdentity> objects, Func<IQueryable<fnClientIdentity>, IOrderedQueryable<fnClientIdentity>> orderBy = null)
        { 
            objects = objects.Where(x => x.CreationTime >= FromDate && x.CreationTime <= ToDate);
            if (HasNote.HasValue)
                objects = objects.Where(x => x.HasNote == HasNote.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, DocumentTypeIds, "DocumentTypeId");
            FilterByValue(ref objects, Statuses, "Status");
            FilterByValue(ref objects, ExpirationTimes, "ExpirationTime");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnClientIdentity> FilterObjects(IQueryable<fnClientIdentity> objects, Func<IQueryable<fnClientIdentity>, IOrderedQueryable<fnClientIdentity>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
        public long SelectedObjectsCount(IQueryable<fnClientIdentity> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }
    }
}
