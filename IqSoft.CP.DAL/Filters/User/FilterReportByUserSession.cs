using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterReportByUserSession : FilterBase<fnUserSession>    
    {
        public int? PartnerId { get; set; }
        public int? UserId { get; set; }
        public int? Type { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation FirstNames { get; set; }
        public FiltersOperation LastNames { get; set; }
        public FiltersOperation Emails { get; set; }
        public FiltersOperation LanguageIds { get; set; }
        public FiltersOperation Ips { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation Types { get; set; }
        public FiltersOperation LogoutTypes { get; set; }
        public FiltersOperation EndTimes { get; set; }
        protected override IQueryable<fnUserSession> CreateQuery(IQueryable<fnUserSession> objects, Func<IQueryable<fnUserSession>, IOrderedQueryable<fnUserSession>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (UserId.HasValue)
                objects = objects.Where(x => x.UserId == UserId.Value);
            if (Type.HasValue)
                objects = objects.Where(x => x.Type == Type.Value);
            objects = objects.Where(x => x.StartTime>=FromDate && x.StartTime <= ToDate);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, LanguageIds, "LanguageId");
            FilterByValue(ref objects, Ips, "Ip");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, LogoutTypes, "LogoutType");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, EndTimes, "EndTime");

            return base.FilteredObjects(objects, orderBy);
        }
        public IQueryable<fnUserSession> FilterObjects(IQueryable<fnUserSession> objects, Func<IQueryable<fnUserSession>, IOrderedQueryable<fnUserSession>> orderBy = null)
        {
           return CreateQuery(objects, orderBy);
        }
        public long SelectedObjectsCount(IQueryable<fnUserSession> userSession, Func<IQueryable<fnUserSession>, IOrderedQueryable<fnUserSession>> orderBy = null)
        {
           return CreateQuery(userSession, orderBy).Count();
        }
    }
}
