using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnPartnerPaymentSetting : FilterBase<fnPartnerPaymentSetting>
    {
        public int? Id { get; set; }

        public int? PartnerId { get; set; }

        public int? PaymentSystemId { get; set; }

        public string CurrencyId { get; set; }

        public int? Status { get; set; }
        public int? Type { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public int? PaymentSystemPriority { get; set; }

        public int? ContentType { get; set; }

        public int? CountryId { get; set; }

        protected override IQueryable<fnPartnerPaymentSetting> CreateQuery(IQueryable<fnPartnerPaymentSetting> objects,
            Func<IQueryable<fnPartnerPaymentSetting>, IOrderedQueryable<fnPartnerPaymentSetting>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (PaymentSystemId.HasValue)
                objects = objects.Where(x => x.PaymentSystemId == PaymentSystemId.Value);
            if (Status.HasValue)
                objects = objects.Where(x => x.State == Status.Value);
            if (Type.HasValue)
                objects = objects.Where(x => x.Type == Type.Value);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
            if (PaymentSystemPriority.HasValue)
                objects = objects.Where(x => x.PaymentSystemPriority == PaymentSystemPriority.Value);
            if (ContentType.HasValue)
                objects = objects.Where(x => x.ContentType == ContentType.Value);
            if (!string.IsNullOrEmpty(CurrencyId))
                objects = objects.Where(x => x.CurrencyId == CurrencyId);

            return base.FilteredObjects(objects);
        }

        public IQueryable<fnPartnerPaymentSetting> FilterObjects(IQueryable<fnPartnerPaymentSetting> partnerPaymentSettings)
        {
            partnerPaymentSettings = CreateQuery(partnerPaymentSettings);
            return partnerPaymentSettings;
        }
    }
}
