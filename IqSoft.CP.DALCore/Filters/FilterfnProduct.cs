using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnProduct : FilterBase<fnProduct>
    {
        public int? ParentId { get; set; }
        public int? ProductId { get; set; }
        public int? Level { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation Names { get; set; }

        public FiltersOperation Descriptions { get; set; }

        public FiltersOperation ExternalIds { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation GameProviderIds { get; set; }

        public FiltersOperation SubProviderIds { get; set; }

        public FiltersOperation IsForDesktops { get; set; }

        public FiltersOperation IsForMobiles { get; set; }
        public FiltersOperation HasDemo { get; set; }

        public FiltersOperation FreeSpinSupports { get; set; }

        public FiltersOperation Jackpots { get; set; }
        public FiltersOperation RTPs { get; set; }

        protected override IQueryable<fnProduct> CreateQuery(IQueryable<fnProduct> objects, Func<IQueryable<fnProduct>, IOrderedQueryable<fnProduct>> orderBy = null)
        {
            if (ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId.Value);
            if (ProductId.HasValue)
                objects = objects.Where(x => x.Id == ProductId.Value);
            if (Level.HasValue)
                objects = objects.Where(x => x.Level == Level.Value);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Names, "Name");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, Descriptions, "NickName");
            FilterByValue(ref objects, ExternalIds, "ExternalId");
            FilterByValue(ref objects, GameProviderIds, "GameProviderId");
            FilterByValue(ref objects, SubProviderIds, "SubproviderId");
            FilterByValue(ref objects, IsForDesktops, "IsForDesktop");
            FilterByValue(ref objects, IsForMobiles, "IsForMobile");
            FilterByValue(ref objects, FreeSpinSupports, "FreeSpinSupport");
            FilterByValue(ref objects, Jackpots, "Jackpot");
            FilterByValue(ref objects, RTPs, "RTP");
            FilterByValue(ref objects, HasDemo, "HasDemo");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnProduct> FilterObjects(IQueryable<fnProduct> fnProducts, Func<IQueryable<fnProduct>, IOrderedQueryable<fnProduct>> orderBy = null)
        {
            return CreateQuery(fnProducts, orderBy);
        }

        public long SelectedObjectsCount(IQueryable<fnProduct> fnProducts)
        {
            fnProducts = CreateQuery(fnProducts);
            return fnProducts.Count();
        }
    }
}
