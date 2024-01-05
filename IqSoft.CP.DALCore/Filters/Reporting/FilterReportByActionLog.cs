using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByActionLog : FilterBase<fnActionLog>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? UserId { get; set; }
        public int? ActionGroupId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ActionNames { get; set; }
        public FiltersOperation ActionGroups { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation Domains { get; set; }
        public FiltersOperation Sources { get; set; }
        public FiltersOperation Ips { get; set; }
        public FiltersOperation Countries { get; set; }
        public FiltersOperation SessionIds { get; set; }
        public FiltersOperation Pages { get; set; }
        public FiltersOperation Languages { get; set; }
        public FiltersOperation ResultCodes { get; set; }
        public FiltersOperation Descriptions { get; set; }

        protected override IQueryable<fnActionLog> CreateQuery(IQueryable<fnActionLog> objects, Func<IQueryable<fnActionLog>, IOrderedQueryable<fnActionLog>> orderBy = null)
        {
            objects = objects.Where(x => x.CreationTime >= FromDate);
            objects = objects.Where(x => x.CreationTime < ToDate);
            if(UserId.HasValue)
                objects = objects.Where(x => x.ObjectId == UserId.Value);
            if (ActionGroupId.HasValue)
                objects = objects.Where(x => x.ActionGroup == ActionGroupId.Value);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ActionNames, "ActionName");
            FilterByValue(ref objects, ActionGroups, "ActionGroup");
            FilterByValue(ref objects, UserIds, "ObjectId");
            FilterByValue(ref objects, Domains, "Domain");
            FilterByValue(ref objects, Sources, "Source");
            FilterByValue(ref objects, Ips, "Ip");
            FilterByValue(ref objects, Countries, "Country");
            FilterByValue(ref objects, SessionIds, "SessionId");
            FilterByValue(ref objects, Languages, "Language");
            FilterByValue(ref objects, ResultCodes, "ResultCode");
            FilterByValue(ref objects, Pages, "Page");
            FilterByValue(ref objects, Descriptions, "Description");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnActionLog> FilterObjects(IQueryable<fnActionLog> objects, Func<IQueryable<fnActionLog>, IOrderedQueryable<fnActionLog>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }

        public long SelectedObjectsCount(IQueryable<fnActionLog> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }
    }
}