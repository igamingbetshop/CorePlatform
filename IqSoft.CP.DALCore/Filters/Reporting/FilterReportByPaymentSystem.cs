using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByPaymentSystem : FilterBase <fnReportByPaymentSystem>
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation PartnerIds { get; set; }

        public FiltersOperation PaymentSystemIds { get; set; }

        public FiltersOperation PaymentSystemNames { get; set; }

        public FiltersOperation Statuses { get; set; }

        public FiltersOperation Counts { get; set; }

        public FiltersOperation TotalAmounts { get; set; }

        protected override IQueryable<fnReportByPaymentSystem> CreateQuery(IQueryable<fnReportByPaymentSystem> objects, Func<IQueryable<fnReportByPaymentSystem>, IOrderedQueryable<fnReportByPaymentSystem>> orderBy = null)
        {
            if(PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, PaymentSystemIds, "PaymentSystemId");
            FilterByValue(ref objects, PaymentSystemNames, "PaymentSystemName");
            FilterByValue(ref objects, Statuses, "Status");
            FilterByValue(ref objects, Counts, "Count");
            FilterByValue(ref objects, TotalAmounts, "TotalAmount");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByPaymentSystem> FilterObjects(IQueryable<fnReportByPaymentSystem> objects, Func<IQueryable<fnReportByPaymentSystem>, IOrderedQueryable<fnReportByPaymentSystem>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
        public long SelectedObjectsCount(IQueryable<fnReportByPaymentSystem> paymentRequests, Func<IQueryable<fnReportByPaymentSystem>, IOrderedQueryable<fnReportByPaymentSystem>> orderBy = null)
        {
            paymentRequests = CreateQuery(paymentRequests, orderBy);
            return paymentRequests.Count();
        }
    }
}
