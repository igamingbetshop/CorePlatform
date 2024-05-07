using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Messages
{
    public class FilterTicket : FilterBase<fnTicket>
    {
        public FiltersOperation Ids { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation PartnerIds { get; set; }

        public FiltersOperation PartnerNames { get; set; }

        public FiltersOperation Subjects { get; set; }

        public FiltersOperation UserIds { get; set; }

        public FiltersOperation UserFirstNames { get; set; }

        public FiltersOperation UserLastNames { get; set; }

        public FiltersOperation Statuses { get; set; }
        public FiltersOperation Types { get; set; }
        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation LastMessageTimes { get; set; }

        public int? State { get; set; }

        public bool UnreadsOnly { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
        public int? PartnerId { get; set; }

        public int? UserId { get; set; }

        protected override IQueryable<fnTicket> CreateQuery(IQueryable<fnTicket> objects, Func<IQueryable<fnTicket>, IOrderedQueryable<fnTicket>> orderBy = null)
        {
            if (State.HasValue)
                objects = objects.Where(x => x.Status == State);
            if (CreatedFrom.HasValue)
            {
                var fromDate = (long)CreatedFrom.Value.Year * 100000000 + CreatedFrom.Value.Month * 1000000 +
                    CreatedFrom.Value.Day * 10000 + CreatedFrom.Value.Hour * 100 + CreatedFrom.Value.Minute;
                objects = objects.Where(x => x.LastMessageDate >= fromDate);
            }
            if (CreatedBefore.HasValue)
            {
                var toDate = (long)CreatedBefore.Value.Year * 100000000 + CreatedBefore.Value.Month * 1000000 +
                    CreatedBefore.Value.Day * 10000 + CreatedBefore.Value.Hour * 100 + CreatedBefore.Value.Minute;
                objects = objects.Where(x => x.LastMessageDate < toDate);
            }
            if (UnreadsOnly)
                objects = objects.Where(x => x.UserUnreadMessagesCount > 0);
            if (PartnerId != null)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (UserId != null)
                objects = objects.Where(x => x.ClientPath.Contains("/" + UserId.Value + "/"));

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, Subjects, "Subject");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, UserFirstNames, "UserFirstName");
            FilterByValue(ref objects, UserLastNames, "UserLastName");
            FilterByValue(ref objects, Statuses, "Status");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastMessageTimes, "LastMessageTime");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnTicket> FilterObjects(IQueryable<fnTicket> tickets, Func<IQueryable<fnTicket>, IOrderedQueryable<fnTicket>> orderBy = null)
        {
            return CreateQuery(tickets, orderBy);
        }

        public long SelectedObjectsCount(IQueryable<fnTicket> tickets)
        {
            tickets = CreateQuery(tickets);
            return tickets.Count();
        }
    }
}
