using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByPopupStatistics : FilterBase<fnPopupStatistics>
    {
        public int? PopupId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation NickNames { get; set; }
        public FiltersOperation Types { get; set; }
        public FiltersOperation DeviceTypes { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }
        public FiltersOperation ViewTypeIds { get; set; }
        public FiltersOperation ViewCounts { get; set; }

        protected override IQueryable<fnPopupStatistics> CreateQuery(IQueryable<fnPopupStatistics> objects, Func<IQueryable<fnPopupStatistics>, IOrderedQueryable<fnPopupStatistics>> orderBy = null)
        {
            if (PopupId.HasValue)
                objects = objects.Where(x => x.Id == PopupId.Value);
            if(FromDate.HasValue)
                objects = objects.Where(x => x.CreationTime >= FromDate);
            if (ToDate.HasValue)
                objects = objects.Where(x => x.CreationTime <= ToDate);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, DeviceTypes, "DeviceType");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            FilterByValue(ref objects, ViewTypeIds, "ViewTypeId");
            FilterByValue(ref objects, ViewCounts, "ViewCount");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnPopupStatistics> FilterObjects(IQueryable<fnPopupStatistics> objects, Func<IQueryable<fnPopupStatistics>, IOrderedQueryable<fnPopupStatistics>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }

        public long SelectedObjectsCount(IQueryable<fnPopupStatistics> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }
    }
}
