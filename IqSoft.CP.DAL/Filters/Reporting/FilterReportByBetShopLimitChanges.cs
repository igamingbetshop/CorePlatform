using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByBetShopLimitChanges : FilterBase<ObjectDataChangeHistory>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation BetShopIds { get; set; }

        public FiltersOperation UserIds { get; set; }

        protected override IQueryable<ObjectDataChangeHistory> CreateQuery(IQueryable<ObjectDataChangeHistory> objects, Func<IQueryable<ObjectDataChangeHistory>, IOrderedQueryable<ObjectDataChangeHistory>> orderBy = null)
        {
            FilterByValue(ref objects, BetShopIds, "BetShopId");
            FilterByValue(ref objects, UserIds, "UserId");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<ObjectDataChangeHistory> FilterObjects(IQueryable<ObjectDataChangeHistory> objectDataChangeHistory, Func<IQueryable<ObjectDataChangeHistory>, IOrderedQueryable<ObjectDataChangeHistory>> orderBy = null)
        {
            return CreateQuery(objectDataChangeHistory, orderBy);
        }

        public long SelectedObjectsCount(IQueryable<ObjectDataChangeHistory> objectDataChangeHistory)
        {
            objectDataChangeHistory = CreateQuery(objectDataChangeHistory);
            return objectDataChangeHistory.Count();
        }
    }
}
