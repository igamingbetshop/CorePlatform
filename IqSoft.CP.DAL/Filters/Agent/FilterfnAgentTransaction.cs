using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnAgentTransaction : FilterBase<fnAgentTransaction>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? UserId { get; set; }
        public int? UserState { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation FromUserIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation ExternalTransactionIds { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation OperationTypeIds { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation ProductNames { get; set; }
        public FiltersOperation TransactionTypes { get; set; }

        protected override IQueryable<fnAgentTransaction> CreateQuery(IQueryable<fnAgentTransaction> objects, 
            Func<IQueryable<fnAgentTransaction>, IOrderedQueryable<fnAgentTransaction>> orderBy = null)
        {
            if (UserId.HasValue)
                objects = objects.Where(x => x.ObjectId == UserId.Value);
            if (UserState.HasValue)
                objects = objects.Where(x => x.UserState == UserState.Value);
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, FromUserIds, "FromUserId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, ExternalTransactionIds, "ExternalTransactionId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, OperationTypeIds, "OperationTypeId");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, TransactionTypes, "TransactionType");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAgentTransaction> FilterObjects(IQueryable<fnAgentTransaction> documents, 
            Func<IQueryable<fnAgentTransaction>, IOrderedQueryable<fnAgentTransaction>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents;
        }

        public long SelectedObjectsCount(IQueryable<fnAgentTransaction> documents, Func<IQueryable<fnAgentTransaction>, IOrderedQueryable<fnAgentTransaction>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents.Count();
        }
    }
}
