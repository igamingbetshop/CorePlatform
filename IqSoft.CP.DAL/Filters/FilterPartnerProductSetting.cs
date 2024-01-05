using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPartnerProductSetting : FilterBase<PartnerProductSetting>
    {
        public int? Id { get; set; }

        public int? PartnerId { get; set; }

        public int? ProductId { get; set; }

        public int? State { get; set; }

        protected override IQueryable<PartnerProductSetting> CreateQuery(IQueryable<PartnerProductSetting> objects, Func<IQueryable<PartnerProductSetting>, IOrderedQueryable<PartnerProductSetting>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (ProductId.HasValue)
                objects = objects.Where(x => x.ProductId == ProductId.Value);
            if (State.HasValue)
                objects = objects.Where(x => x.State == State.Value);
            return base.FilteredObjects(objects);
        }

        public IQueryable<PartnerProductSetting> FilterObjects(IQueryable<PartnerProductSetting> partnerProductSettings)
        {
            partnerProductSettings = CreateQuery(partnerProductSettings);
            return partnerProductSettings;
        }

        public long SelectedObjectsCount(IQueryable<PartnerProductSetting> partnerProductSettings)
        {
            partnerProductSettings = CreateQuery(partnerProductSettings);
            return partnerProductSettings.Count();
        }
    }
}
