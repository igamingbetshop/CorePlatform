using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnProduct : FilterBase<fnProduct>
    {
        public int? PartnerId { get; set; }
        public int? ParentId { get; set; }
        public int? ProductId { get; set; }
        public int? Level { get; set; }
        public string Pattern { get; set; }
        public bool? HasImages { get; set; }
        public bool? IsForMobile { get; set; }
        public bool? IsForDesktop { get; set; }
        public bool? FreeSpinSupport { get; set; }
        public bool? IsProviderActive { get; set; }
        public string Path { get; set; }
        public List<int> GroupIds { get; set; }

        public FiltersOperation Ids { get; set; }
        public FiltersOperation Names { get; set; }
        public FiltersOperation Descriptions { get; set; }
        public FiltersOperation ExternalIds { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation GameProviderIds { get; set; }
        public FiltersOperation SubProviderIds { get; set; }
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
            if (HasImages.HasValue)
                objects = objects.Where(x => x.HasImages == HasImages.Value);
            if (IsProviderActive.HasValue)
                objects = objects.Where(x => x.IsProviderActive == null || x.IsProviderActive == IsProviderActive.Value );
            if (IsForDesktop.HasValue)
                objects = objects.Where(x => x.IsForDesktop == IsForDesktop);
            if (IsForMobile.HasValue)
                objects = objects.Where(x => x.IsForMobile == IsForMobile);
            if (FreeSpinSupport.HasValue)
                objects = objects.Where(x => x.GameProviderId == null || x.FreeSpinSupport == FreeSpinSupport.Value);
            if (!string.IsNullOrEmpty(Pattern))
                objects = objects.Where(x => x.GameProviderId != null && (x.Name.Contains(Pattern) || x.NickName.Contains(Pattern)));
            if (!string.IsNullOrEmpty(Path))
                objects = objects.Where(x => x.Path.Contains(Path));
            if (PartnerId.HasValue)
            {
                if(GroupIds != null && GroupIds.Any())
                    objects = objects.Where(x => x.PartnerId != null || (x.GameProviderId == null && GroupIds.Contains(x.Id)));
                else
                    objects = objects.Where(x => x.PartnerId != null || x.GameProviderId == null);
            }
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Names, "Name");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, Descriptions, "NickName");
            FilterByValue(ref objects, ExternalIds, "ExternalId");
            FilterByValue(ref objects, GameProviderIds, "GameProviderId");
            FilterByValue(ref objects, SubProviderIds, "SubproviderId");
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
