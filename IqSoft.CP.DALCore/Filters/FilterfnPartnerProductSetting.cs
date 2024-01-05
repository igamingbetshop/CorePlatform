using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnPartnerProductSetting : FilterBase<fnPartnerProductSetting>
    {
        public int? PartnerId { get; set; }
        public List<int> ProductSettingIds { get; set; }
        public int? ProviderId { get; set; }
        public FiltersOperation IsForMobile { get; set; }
        public FiltersOperation IsForDesktop { get; set; }
        public FiltersOperation HasDemo { get; set; }
        public FiltersOperation ProductIsLeaf { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation OpenModes { get; set; }
        public FiltersOperation CategoryIds { get; set; }
        public FiltersOperation RTPs { get; set; }
        public FiltersOperation Volatilities { get; set; }
        public FiltersOperation Ratings { get; set; }
        public FiltersOperation ProductDescriptions { get; set; }
        public FiltersOperation ProductNames { get; set; }
        public FiltersOperation ProductExternalIds { get; set; }
        public FiltersOperation ProductGameProviders { get; set; }
        public FiltersOperation SubProviderIds { get; set; }
        public FiltersOperation ProductParents { get; set; }
        public FiltersOperation Percents { get; set; }
        public FiltersOperation Jackpots { get; set; }

        protected override IQueryable<fnPartnerProductSetting> CreateQuery(IQueryable<fnPartnerProductSetting> objects, Func<IQueryable<fnPartnerProductSetting>, IOrderedQueryable<fnPartnerProductSetting>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
            if (ProviderId.HasValue)
                objects = objects.Where(x => x.ProductGameProviderId == ProviderId);
            if (ProductSettingIds != null && ProductSettingIds.Count > 0)
                objects = objects.Where(x => ProductSettingIds.Contains(x.Id));

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, OpenModes, "OpenMode");
            FilterByValue(ref objects, CategoryIds, "CategoryId");
            FilterByValue(ref objects, IsForMobile, "IsForMobile");
            FilterByValue(ref objects, IsForDesktop, "IsForDesktop");
            FilterByValue(ref objects, ProductIsLeaf, "ProductIsLeaf");
            FilterByValue(ref objects, Ratings, "Rating");
            FilterByValue(ref objects, RTPs, "RTP");
            FilterByValue(ref objects, Volatilities, "Volatility");
            FilterByValue(ref objects, ProductDescriptions, "ProductNickName");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, ProductExternalIds, "ProductExternalId");
            FilterByValue(ref objects, ProductGameProviders, "ProductGameProviderId");
            FilterByValue(ref objects, SubProviderIds, "SubproviderId");
            FilterByValue(ref objects, ProductParents, "ProductParentId");
            FilterByValue(ref objects, Percents, "Percent");
            FilterByValue(ref objects, Jackpots, "Jackpot");
            FilterByValue(ref objects, HasDemo, "HasDemo");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnPartnerProductSetting> FilterObjects(IQueryable<fnPartnerProductSetting> partnerProductSettings, Func<IQueryable<fnPartnerProductSetting>, IOrderedQueryable<fnPartnerProductSetting>> orderBy = null)
        {
            partnerProductSettings = CreateQuery(partnerProductSettings, orderBy);
            return partnerProductSettings;
        }

        public long SelectedObjectsCount(IQueryable<fnPartnerProductSetting> partnerProductSettings)
        {
            partnerProductSettings = CreateQuery(partnerProductSettings);
            return partnerProductSettings.Count();
        }
    }
}