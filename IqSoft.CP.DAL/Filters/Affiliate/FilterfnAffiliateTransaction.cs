using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
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

        protected override IQueryable<fnAffiliateTransaction> CreateQuery(IQueryable<fnAffiliateTransaction> objects, 
            Func<IQueryable<fnAffiliateTransaction>, IOrderedQueryable<fnAffiliateTransaction>> orderBy = null)
        {
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, ExternalTransactionIds, "ExternalTransactionId");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, TransactionTypes, "TransactionType");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAffiliateTransaction> FilterObjects(IQueryable<fnAffiliateTransaction> documents, 
            Func<IQueryable<fnAffiliateTransaction>, IOrderedQueryable<fnAffiliateTransaction>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents;
        }

        public long SelectedObjectsCount(IQueryable<fnAffiliateTransaction> documents, Func<IQueryable<fnAffiliateTransaction>, IOrderedQueryable<fnAffiliateTransaction>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents.Count();
        }
    }
}
