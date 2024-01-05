using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterDocument : FilterBase<Document>
    {
        public string ExternalTransactionId { get; set; }

        public long? PaymentRequestId { get; set; }

        public int? OperationTypeId { get; set; }

        public List<int> OperationTypeIds { get; set; }

        public int? PartnerPaymentSettingId { get; set; }

        public int? GameProviderId { get; set; }

        public int? ProductId { get; set; }

        public long? Barcode { get; set; }

        public int? State { get; set; }

        protected override IQueryable<Document> CreateQuery(IQueryable<Document> objects, Func<IQueryable<Document>, IOrderedQueryable<Document>> orderBy = null)
        {
            if (!string.IsNullOrWhiteSpace(ExternalTransactionId))
                objects = objects.Where(x => x.ExternalTransactionId == ExternalTransactionId);
            if (PaymentRequestId.HasValue)
                objects = objects.Where(x => x.PaymentRequestId == PaymentRequestId.Value);
            if (OperationTypeId.HasValue)
                objects = objects.Where(x => x.OperationTypeId == OperationTypeId.Value);
            if (OperationTypeIds != null && OperationTypeIds.Count != 0)
                objects = objects.Where(x => OperationTypeIds.Contains(x.OperationTypeId));
            if (PartnerPaymentSettingId.HasValue)
                objects = objects.Where(x => x.PartnerPaymentSettingId == PartnerPaymentSettingId.Value);
            if (GameProviderId.HasValue)
                objects = objects.Where(x => x.GameProviderId == GameProviderId.Value);
            if (Barcode.HasValue)
            {
                Barcode -= 1000000000000;
                Barcode /= 10;
                objects = objects.Where(x => x.Id == Barcode);
            }
            if (ProductId.HasValue)
                objects = objects.Where(x => x.ProductId == ProductId.Value);
            if (State.HasValue)
                objects = objects.Where(x => x.State == State.Value);

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Document> FilterObjects(IQueryable<Document> documents, Func<IQueryable<Document>, IOrderedQueryable<Document>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents;
        }

        public long SelectedObjectsCount(IQueryable<Document> documents, Func<IQueryable<Document>, IOrderedQueryable<Document>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents.Count();
        }
    }
}
