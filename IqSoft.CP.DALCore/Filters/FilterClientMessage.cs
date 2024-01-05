using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterClientMessage : FilterBase<fnClientMessage>
    {
        public FiltersOperation Ids { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation PartnerIds { get; set; }

        public FiltersOperation Subjects { get; set; }
        public FiltersOperation Statuses { get; set; }
        public FiltersOperation MobileOrEmails { get; set; }


        public List<int> Types { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<fnClientMessage> CreateQuery(IQueryable<fnClientMessage> objects, Func<IQueryable<fnClientMessage>, IOrderedQueryable<fnClientMessage>> orderBy = null)
        {
            objects = objects.Where(x => Types.Contains(x.MessageType));
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x. CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, Subjects, "Subject");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, Statuses, "Status");
            FilterByValue(ref objects, MobileOrEmails, "MobileOrEmail");

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
