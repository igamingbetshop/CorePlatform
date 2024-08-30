using IqSoft.CP.Common.Models;
using System.Linq;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterfnDuplicateClient : FilterBase<fnDuplicateClient>
    {
        public int ClientId { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation DuplicatedClientIds { get; set; }
        public FiltersOperation DuplicatedDatas { get; set; }
        public FiltersOperation MatchDates { get; set; }

        public override void CreateQuery(ref IQueryable<fnDuplicateClient> objects, bool orderBy, bool orderByDate = false)
        {
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
