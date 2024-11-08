﻿using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnPartnerProductSetting : FilterBase<fnPartnerProductSetting>
    {
        public int? PartnerId { get; set; }
        public int? State { get; set; }
        public bool? HasImages { get; set; }
        public bool? IsForMobile { get; set; }
        public bool? IsForDesktop { get; set; }
        public bool? HasDemo { get; set; }
        public List<int> ProductSettingIds { get; set; }
        public int? ProviderId { get; set; }
        public int? ProductId { get; set; }
        public string CategoryIds { get; set; }
        public bool? IsProviderActive { get; set; }
        public bool? FreeSpinSupport { get; set; }
        public int? ParentId { get; set; }
        public string Pattern { get; set; }
        public string Path { get; set; }
        public FiltersOperation ProductIsLeaf { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation OpenModes { get; set; }
        public FiltersOperation RTPs { get; set; }
        public FiltersOperation ExternalIds { get; set; }
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
            if (ProductId.HasValue)
                objects = objects.Where(x => x.ProductId == ProductId);
            if (ProviderId.HasValue)
                objects = objects.Where(x => x.ProductGameProviderId == ProviderId);
            if (State.HasValue)
                objects = objects.Where(x => x.State == State);
            if (ProductSettingIds != null && ProductSettingIds.Count > 0)
                objects = objects.Where(x => ProductSettingIds.Contains(x.Id));
            if (HasImages.HasValue)
                objects = objects.Where(x => x.HasImages == HasImages);
            if (HasDemo.HasValue)
                objects = objects.Where(x => x.HasDemo == HasDemo);
            if (IsForDesktop.HasValue)
                objects = objects.Where(x => x.IsForDesktop == IsForDesktop);
            if (IsForMobile.HasValue)
                objects = objects.Where(x => x.IsForMobile == IsForMobile);
            if (FreeSpinSupport.HasValue)
                objects = objects.Where(x => x.ProductGameProviderId == null || x.FreeSpinSupport == FreeSpinSupport.Value);
            if (IsProviderActive.HasValue)
                objects = objects.Where(x => x.IsProviderActive == null || x.IsProviderActive == IsProviderActive.Value);
            if (ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId);
            if (!string.IsNullOrEmpty(Path))
                objects = objects.Where(x => x.Path.Contains(Path));
            if (!string.IsNullOrEmpty(Pattern))
                objects = objects.Where(x => x.ProductGameProviderId != null && (x.ProductName.Contains(Pattern) || x.ProductNickName.Contains(Pattern)));

            if (!string.IsNullOrEmpty(CategoryIds))
                objects = objects.Where(x => x.CategoryIds.Contains("[" + CategoryIds + "]") || x.CategoryIds.Contains("," + CategoryIds + ",") ||
                                             x.CategoryIds.Contains("[" + CategoryIds + ",") || x.CategoryIds.Contains("," + CategoryIds + "]"));
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, OpenModes, "OpenMode");
            FilterByValue(ref objects, ProductIsLeaf, "ProductIsLeaf");
            FilterByValue(ref objects, Ratings, "Rating");
            FilterByValue(ref objects, RTPs, "RTP");
            FilterByValue(ref objects, RTPs, "ExternalId");
            FilterByValue(ref objects, Volatilities, "Volatility");
            FilterByValue(ref objects, ProductDescriptions, "ProductNickName");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, ProductExternalIds, "ProductExternalId");
            FilterByValue(ref objects, ProductGameProviders, "ProductGameProviderId");
            FilterByValue(ref objects, SubProviderIds, "SubproviderId");
            FilterByValue(ref objects, ProductParents, "ProductParentId");
            FilterByValue(ref objects, Percents, "Percent");
            FilterByValue(ref objects, Jackpots, "Jackpot");

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