using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterfnDocument : FilterBase<fnDocument>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ClientId { get; set; }
        public int? PartnerId { get; set; }
        public long? AccountId { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ExternalTransactionIds { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation OperationTypeIds { get; set; }
        public FiltersOperation PaymentRequestIds { get; set; }
        public FiltersOperation PaymentSystemIds { get; set; }
        public FiltersOperation PaymentSystemNames { get; set; }
        public FiltersOperation RoundIds { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation ProductNames { get; set; }
        public FiltersOperation GameProviderIds { get; set; }
        public FiltersOperation GameProviderNames { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }

        public override void CreateQuery(ref IQueryable<fnDocument> objects, Func<IQueryable<fnDocument>, IOrderedQueryable<fnDocument>> orderBy = null)
        {
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
            if (AccountId.HasValue)
                objects = objects.Where(x => x.AccountId == AccountId);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ExternalTransactionIds, "ExternalTransactionId");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, OperationTypeIds, "OperationTypeId");
            FilterByValue(ref objects, PaymentRequestIds, "PaymentRequestId");
            FilterByValue(ref objects, PaymentSystemIds, "PaymentSystemId");
            FilterByValue(ref objects, PaymentSystemNames, "PaymentSystemName");
            FilterByValue(ref objects, RoundIds, "RoundId");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, GameProviderIds, "GameProviderId");
            FilterByValue(ref objects, GameProviderNames, "GameProviderName");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            base.FilteredObjects(ref objects, orderBy);
        }

        public IQueryable<fnDocument> FilterObjects(IQueryable<fnDocument> documents, Func<IQueryable<fnDocument>, IOrderedQueryable<fnDocument>> orderBy = null)
        {
            CreateQuery(ref documents, orderBy);
            return documents;
        }

        public long SelectedObjectsCount(IQueryable<fnDocument> documents, Func<IQueryable<fnDocument>, IOrderedQueryable<fnDocument>> orderBy = null)
        {
            CreateQuery(ref documents, orderBy);
            return documents.Count();
        }
    }
}