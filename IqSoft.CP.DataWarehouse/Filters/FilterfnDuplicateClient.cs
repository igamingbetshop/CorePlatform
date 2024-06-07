using IqSoft.CP.Common.Models;
using System;
using System.Linq;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterfnDuplicateClient : FilterBase<fnDuplicateClient>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? PartnerId { get; set; }
        public int? ClientId { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation DuplicatedClientIds { get; set; }
        public FiltersOperation DuplicatedDatas { get; set; }
        public FiltersOperation MatchDates { get; set; }

        public override void CreateQuery(ref IQueryable<fnDuplicateClient> objects, bool orderBy, bool orderByDate = false)
        {
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, DuplicatedClientIds, "DuplicatedClientId");
            FilterByValue(ref objects, DuplicatedDatas, "DuplicatedData");
            FilterByValue(ref objects, MatchDates, "LastUpdateTime");
            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }
        public IQueryable<fnDuplicateClient> FilterObjects(IQueryable<fnDuplicateClient> duplicateClient, bool ordering)
        {
            CreateQuery(ref duplicateClient, ordering);
            return duplicateClient;
        }

        public long SelectedObjectsCount(IQueryable<fnDuplicateClient> duplicateClient, bool ordering)
        {
            CreateQuery(ref duplicateClient, ordering);
            return duplicateClient.Count();
        }

    }
}
