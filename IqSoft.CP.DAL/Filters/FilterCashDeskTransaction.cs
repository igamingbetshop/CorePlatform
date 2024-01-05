using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterCashDeskTransaction : FilterBase<fnCashDeskTransaction>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation BetShopNames { get; set; }

        public FiltersOperation CashierIds { get; set; }

        public FiltersOperation CashDeskIds { get; set; }

        public FiltersOperation BetShopIds { get; set; }

        public FiltersOperation OperationTypeNames { get; set; }

        public FiltersOperation Amounts { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation CreationTimes { get; set; }

        public FiltersOperation States { get; set; }

        protected override IQueryable<fnCashDeskTransaction> CreateQuery(IQueryable<fnCashDeskTransaction> objects, Func<IQueryable<fnCashDeskTransaction>
            , IOrderedQueryable<fnCashDeskTransaction>> orderBy = null)
        {
            objects = objects.Where(x => x.CreationTime >= FromDate);
            objects = objects.Where(x => x.CreationTime < ToDate);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, BetShopNames, "BetShopName");
            FilterByValue(ref objects, CashierIds, "CashierId");
            FilterByValue(ref objects, CashDeskIds, "CashDeskId");
            FilterByValue(ref objects, BetShopIds, "BetShopId");
            FilterByValue(ref objects, OperationTypeNames, "OperationTypeName");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, States, "State");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnCashDeskTransaction> FilterObjects(IQueryable<fnCashDeskTransaction> documents,
            Func<IQueryable<fnCashDeskTransaction>, IOrderedQueryable<fnCashDeskTransaction>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents;
        }

        public long SelectedObjectsCount(IQueryable<fnCashDeskTransaction> documents)
        {
            documents = CreateQuery(documents);
            return documents.Count();
        }
    }
}
