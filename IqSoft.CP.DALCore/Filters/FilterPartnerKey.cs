using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPartnerKey : FilterBase<PartnerKey>
    {
        public int? PartnerId { get; set; }
        public int? PaymentSystemId { get; set; }
        public int? GameProviderId { get; set; }
        public string Name { get; set; }

        protected override IQueryable<PartnerKey> CreateQuery(IQueryable<PartnerKey> objects, Func<IQueryable<PartnerKey>, IOrderedQueryable<PartnerKey>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (PaymentSystemId.HasValue)
                objects = objects.Where(x => x.PaymentSystemId == PaymentSystemId.Value);
            if (GameProviderId.HasValue)
                objects = objects.Where(x => x.GameProviderId == GameProviderId.Value);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name == Name);
            return base.FilteredObjects(objects);
        }

        public IQueryable<PartnerKey> FilterObjects(IQueryable<PartnerKey> partnerKeys)
        {
            partnerKeys = CreateQuery(partnerKeys);
            return partnerKeys;
        }

        public long SelectedObjectsCount(IQueryable<PartnerKey> partnerKeys)
        {
            partnerKeys = CreateQuery(partnerKeys);
            return partnerKeys.Count();
        }
    }
}
