using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterReportByClientSession : FilterBase<ClientSession>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ClientId { get; set; }
        public int? ProductId { get; set; }
        public long? ParentId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation LanguageIds { get; set; }
        public FiltersOperation Ips { get; set; }
        public FiltersOperation Countries { get; set; }
        public FiltersOperation DeviceTypes { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation LogoutTypes { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation StartTimes { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }
        public FiltersOperation EndTimes { get; set; }

        public override void CreateQuery(ref IQueryable<ClientSession> objects, bool orderBy, bool orderByDate = false)
        {
            if(FromDate.HasValue)
            objects = objects.Where(x => x.StartTime >= FromDate);
            if(ToDate.HasValue)
            objects = objects.Where(x => x.StartTime < ToDate);
            if(ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId);
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId);
            if (ProductId.HasValue)
                objects = objects.Where(x => x.ProductId == ProductId);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, LanguageIds, "LanguageId");
            FilterByValue(ref objects, Ips, "Ip");
            FilterByValue(ref objects, Countries, "Country");
            FilterByValue(ref objects, DeviceTypes, "DeviceType");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, LogoutTypes, "LogoutType");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, StartTimes, "StartTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            FilterByValue(ref objects, EndTimes, "EndTime");
            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }

        public IQueryable<ClientSession> FilterObjects(IQueryable<ClientSession> sessions, bool ordering)
        {
            CreateQuery(ref sessions, ordering);
            return sessions;
        }
        public long SelectedObjectsCount(IQueryable<ClientSession> sessions, bool ordering)
        {
            CreateQuery(ref sessions, ordering);
            return sessions.Count();
        }
    }
}
