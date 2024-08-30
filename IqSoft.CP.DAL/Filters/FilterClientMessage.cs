using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterClientMessage : FilterBase<fnClientMessage>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<int> Types { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation MessageIds { get; set; }
        public FiltersOperation Ids { get; set; } // ClientId
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation MobileOrEmails { get; set; }
        public FiltersOperation Subjects { get; set; }
        public FiltersOperation Messages { get; set; }
        public FiltersOperation MessageTypes { get; set; }
        public FiltersOperation Statuses { get; set; }
        public FiltersOperation CreationTimes { get; set; }

        protected override IQueryable<fnClientMessage> CreateQuery(IQueryable<fnClientMessage> objects, Func<IQueryable<fnClientMessage>, IOrderedQueryable<fnClientMessage>> orderBy = null)
        {
            if(Types != null && Types.Any())
                objects = objects.Where(x => Types.Contains(x.MessageType));
            if (FromDate.HasValue)
                objects = objects.Where(x => x. CreationTime >= FromDate.Value);
            if (ToDate.HasValue)
                objects = objects.Where(x => x.CreationTime < ToDate.Value);

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, MessageIds, "MessageId");
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, MobileOrEmails, "MobileOrEmail");
            FilterByValue(ref objects, Subjects, "Subject");
            FilterByValue(ref objects, Messages, "Message");
            FilterByValue(ref objects, MessageTypes, "MessageType");
            FilterByValue(ref objects, Statuses, "Status");
            FilterByValue(ref objects, CreationTimes, "CreationTime");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnClientMessage> FilterObjects(IQueryable<fnClientMessage> clientMessages, Func<IQueryable<fnClientMessage>, IOrderedQueryable<fnClientMessage>> orderBy = null)
        {
            return CreateQuery(clientMessages, orderBy);
        }

        public long SelectedObjectsCount(IQueryable<fnClientMessage> clientMessages)
        {
            clientMessages = CreateQuery(clientMessages);
            return clientMessages.Count();
        }
    }
}
