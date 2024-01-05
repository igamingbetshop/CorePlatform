using IqSoft.CP.Common.Enums;
using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterAnnouncement : FilterBase<fnAnnouncement>
    {
        public int? PartnerId { get; set; }
        public int? ReceiverId { get; set; }
        public int? Type { get; set; }
        public int? ReceiverTypeId { get; set; }
        public int? AgentId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation ReceiverIds { get; set; }
        public FiltersOperation Types { get; set; }
        public FiltersOperation States { get; set; }

        protected override IQueryable<fnAnnouncement> CreateQuery(IQueryable<fnAnnouncement> objects, Func<IQueryable<fnAnnouncement>, IOrderedQueryable<fnAnnouncement>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (ReceiverId.HasValue)
                objects = objects.Where(x => x.ReceiverId == ReceiverId.Value || x.ReceiverId == null);
            if (ReceiverTypeId.HasValue)
                objects = objects.Where(x => x.ReceiverTypeId == ReceiverTypeId.Value);
            if (Type.HasValue)
                objects = objects.Where(x => x.Type == Type.Value);
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);

            if (AgentId == null)
                objects = objects.Where(x => x.UserId == null || x.UserType == (int)UserTypes.AdminUser);
            else
                objects = objects.Where(x => x.UserId == null || x.UserType == (int)UserTypes.AdminUser || x.UserId == AgentId.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, ReceiverIds, "ReceiverId");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, States, "State");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAnnouncement> FilterObjects(IQueryable<fnAnnouncement> announcements, Func<IQueryable<fnAnnouncement>, IOrderedQueryable<fnAnnouncement>> orderBy = null)
        {
            announcements = CreateQuery(announcements, orderBy);
            return announcements;
        }

        public long SelectedObjectsCount(IQueryable<fnAnnouncement> announcements, Func<IQueryable<fnAnnouncement>, IOrderedQueryable<fnAnnouncement>> orderBy = null)
        {
            announcements = CreateQuery(announcements, orderBy);
            return announcements.Count();
        }
    }
}
