using IqSoft.CP.Common.Models;
using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPopup : FilterBase<Popup>
    {
        public int? PartnerId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation NickNames { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation Types { get; set; }
        public FiltersOperation Orders { get; set; }
        public FiltersOperation Pages { get; set; }
        public FiltersOperation StartDates { get; set; }
        public FiltersOperation FinishDates { get; set; }
        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }

        protected override IQueryable<Popup> CreateQuery(IQueryable<Popup> objects, Func<IQueryable<Popup>, IOrderedQueryable<Popup>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, Orders, "Order");
            FilterByValue(ref objects, Pages, "Page");
            FilterByValue(ref objects, StartDates, "StartDate");
            FilterByValue(ref objects, FinishDates, "FinishDate");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Popup> FilterObjects(IQueryable<Popup> popups, Func<IQueryable<Popup>, IOrderedQueryable<Popup>> orderBy = null)
        {
            return CreateQuery(popups, orderBy);
        }
        public long SelectedObjectsCount(IQueryable<Popup> popup)
        {
            return CreateQuery(popup).Count();
        }
    }
}
