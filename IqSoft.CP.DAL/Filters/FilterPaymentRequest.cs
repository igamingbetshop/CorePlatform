using IqSoft.CP.Common.Enums;
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

        public int? AffiliateId { get; set; }

        protected override IQueryable<PaymentRequest> CreateQuery(IQueryable<PaymentRequest> objects,
            Func<IQueryable<PaymentRequest>, IOrderedQueryable<PaymentRequest>> orderBy = null)
        {
            objects = objects.Where(x => x.Date >= FromDate);
            objects = objects.Where(x => x.Date < ToDate);
            var path = "/" + AgentId + "/";
            if (Type.HasValue && Type == (int)PaymentRequestTypes.Deposit)
                objects = objects.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit);
            else if (Type.HasValue)
                objects = objects.Where(x => x.Type == Type.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.Client.PartnerId == PartnerId.Value);
            if (AgentId.HasValue)
                objects = objects.Where(x => x.Client.User.Path.Contains(path));
            if (AffiliateId.HasValue)
                objects = objects.Where(x => x.Client.AffiliateReferral.AffiliateId == AffiliateId.Value.ToString() && x.Client.AffiliateReferral.AffiliatePlatformId == PartnerId.Value * 100 &&
                                             x.Client.AffiliateReferral.Type == (int)AffiliateReferralTypes.InternalAffiliatePlatform);

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
