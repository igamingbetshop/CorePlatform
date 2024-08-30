using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterfnAffiliateTransaction : FilterBase<fnAffiliateTransaction>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation ExternalTransactionIds { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation ProductNames { get; set; }
        public FiltersOperation TransactionTypes { get; set; }
        public FiltersOperation CreationTimes { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }

        public override void CreateQuery(ref IQueryable<fnAffiliateTransaction> objects, bool orderBy, bool orderByDate = false)
        {
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, ExternalTransactionIds, "ExternalTransactionId");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, TransactionTypes, "TransactionType");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }

        public IQueryable<fnAffiliateTransaction> FilterObjects(IQueryable<fnAffiliateTransaction> affiliateTransaction, bool ordering)
        {
            CreateQuery(ref affiliateTransaction, ordering);
            return affiliateTransaction;
        }

        public long SelectedObjectsCount(IQueryable<fnAffiliateTransaction> affiliateTransaction, bool ordering)
        {
            CreateQuery(ref affiliateTransaction, ordering);
            return affiliateTransaction.Count();
        }
    }
}
