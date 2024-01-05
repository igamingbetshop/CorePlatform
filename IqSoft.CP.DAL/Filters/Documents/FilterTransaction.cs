using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterTransaction : FilterBase<Transaction>
    {
        public int? Id { get; set; }

        public long? DocumentId { get; set; }

        public string CurrencyId { get; set; }

        public List<long> AccountIds { get; set; }

        public int? OperationTypeId { get; set; }

        public List<int> OperationTypeIds { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        protected override IQueryable<Transaction> CreateQuery(IQueryable<Transaction> objects, Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>> orderBy = null)
        {
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;

            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);

            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (DocumentId.HasValue)
                objects = objects.Where(x => x.DocumentId == DocumentId.Value);
            if (AccountIds != null && AccountIds.Count > 0)
                objects = objects.Where(x => AccountIds.Contains(x.AccountId));
            if (OperationTypeId.HasValue)
                objects = objects.Where(x => x.OperationTypeId == OperationTypeId.Value);
            if (OperationTypeIds != null && OperationTypeIds.Count != 0)
                objects = objects.Where(x => OperationTypeIds.Contains(x.OperationTypeId));

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Transaction> FilterObjects(IQueryable<Transaction> transactions, Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>> orderBy = null)
        {
            transactions = CreateQuery(transactions, orderBy);
            return transactions;
        }

        public long SelectedObjectsCount(IQueryable<Transaction> transactions)
        {
            transactions = CreateQuery(transactions);
            return transactions.Count();
        }
    }
}
