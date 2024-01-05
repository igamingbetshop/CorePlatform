using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPartnerPaymentSetting : FilterBase<PartnerPaymentSetting>
    {
        public int? Id { get; set; }

        public int? PartnerId { get; set; }

        public int? PaymentSystemId { get; set; }

        public string CurrencyId { get; set; }

        public int? Status { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<PartnerPaymentSetting> CreateQuery(IQueryable<PartnerPaymentSetting> objects,
            Func<IQueryable<PartnerPaymentSetting>, IOrderedQueryable<PartnerPaymentSetting>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (PaymentSystemId.HasValue)
                objects = objects.Where(x => x.PaymentSystemId == PaymentSystemId.Value);
            if (string.IsNullOrWhiteSpace(CurrencyId))
                objects = objects.Where(x => x.CurrencyId == CurrencyId);
            if (Status.HasValue)
                objects = objects.Where(x => x.State == Status.Value);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            return base.FilteredObjects(objects);
        }

        public IQueryable<PartnerPaymentSetting> FilterObjects(IQueryable<PartnerPaymentSetting> partnerPaymentSettings)
        {
            partnerPaymentSettings = CreateQuery(partnerPaymentSettings);
            return partnerPaymentSettings;
        }

        public long SelectedObjectsCount(IQueryable<PartnerPaymentSetting> partnerPaymentSettings)
        {
            partnerPaymentSettings = CreateQuery(partnerPaymentSettings);
            return partnerPaymentSettings.Count();
        }
    }
}
