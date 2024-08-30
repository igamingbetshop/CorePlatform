using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Clients
{
    public class FilterReportByfnClientSession : FilterBase<fnClientSession>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? PartnerId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation FirstNames { get; set; }
        public FiltersOperation LastNames { get; set; }
        public FiltersOperation UserNames { get; set; }
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

        protected override IQueryable<fnClientSession> CreateQuery(IQueryable<fnClientSession> objects, Func<IQueryable<fnClientSession>, IOrderedQueryable<fnClientSession>> orderBy = null)
        {
            objects = objects.Where(x => x.StartTime >= FromDate);
            objects = objects.Where(x => x.StartTime < ToDate);
            if(PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
 
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, UserNames, "UserName");
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

            return FilteredObjects(objects, orderBy);
        }
        
        public IQueryable<fnClientSession> FilterObjects(IQueryable<fnClientSession> sessions, Func<IQueryable<fnClientSession>, IOrderedQueryable<fnClientSession>> orderBy = null)
        {
            sessions = CreateQuery(sessions, orderBy);
            return sessions;
        }

        public long SelectedObjectsCount(IQueryable<fnClientSession> sessions, Func<IQueryable<fnClientSession>, IOrderedQueryable<fnClientSession>> orderBy = null)
        {
            sessions = CreateQuery(sessions, orderBy);
            return sessions.Count();
        }
    }
}
