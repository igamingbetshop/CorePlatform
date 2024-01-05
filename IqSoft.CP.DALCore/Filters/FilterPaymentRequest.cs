using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPaymentRequest : FilterBase<PaymentRequest>
    {
		public int? PartnerId { get; set; }

		public long FromDate { get; set; }

        public long ToDate { get; set; }

		public int? Type { get; set; }

        public int? AgentId { get; set; }

        protected override IQueryable<PaymentRequest> CreateQuery(IQueryable<PaymentRequest> objects,
            Func<IQueryable<PaymentRequest>, IOrderedQueryable<PaymentRequest>> orderBy = null)
        {
			objects = objects.Where(x => x.Date >= FromDate);
			objects = objects.Where(x => x.Date < ToDate);
            if (Type.HasValue)
                objects = objects.Where(x => x.Type == Type.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.Client.PartnerId == PartnerId.Value);
            if (AgentId.HasValue)
                objects = objects.Where(x => x.Client.User.Path.Contains("/" + AgentId + "/"));


            return FilteredObjects(objects, orderBy);
        }

        public IQueryable<PaymentRequest> FilterObjects(IQueryable<PaymentRequest> paymentRequests, Func<IQueryable<PaymentRequest>, IOrderedQueryable<PaymentRequest>> orderBy = null)
        {
            paymentRequests = CreateQuery(paymentRequests, orderBy);
            return paymentRequests;
        }

        public long SelectedObjectsCount(IQueryable<PaymentRequest> paymentRequests)
        {
            paymentRequests = CreateQuery(paymentRequests);
            return paymentRequests.Count();
        }
    }
}
