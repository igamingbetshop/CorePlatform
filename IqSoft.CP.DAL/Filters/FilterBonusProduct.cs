using IqSoft.CP.Common.Models;
using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterBonusProduct : FilterBase<BonusProduct>
    {
        public int? BonusId { get; set; }
        public FiltersOperation BonusIds { get; set; }
        public FiltersOperation Percents { get; set; }
        public FiltersOperation ProductIds { get; set; }
        public FiltersOperation Counts { get; set; }
        public FiltersOperation Lines { get; set; }
        public FiltersOperation Coins { get; set; }
        public FiltersOperation CoinValues { get; set; }
        public FiltersOperation BetValues { get; set; }

        protected override IQueryable<BonusProduct> CreateQuery(IQueryable<BonusProduct> objects, Func<IQueryable<BonusProduct>, IOrderedQueryable<BonusProduct>> orderBy = null)
        {
            if (BonusId.HasValue)
                objects = objects.Where(x => x.BonusId == BonusId.Value);
            FilterByValue(ref objects, BonusIds, "BonusId");
            FilterByValue(ref objects, Percents, "Percent");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, Counts, "Count");
            FilterByValue(ref objects, Lines, "Lines");
            FilterByValue(ref objects, Coins, "Coins");
            FilterByValue(ref objects, CoinValues, "CoinValue");
            FilterByValue(ref objects, BetValues, "BetValues");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<BonusProduct> FilterObjects(IQueryable<BonusProduct> fnProducts, Func<IQueryable<BonusProduct>, IOrderedQueryable<BonusProduct>> orderBy = null)
        {
            return CreateQuery(fnProducts, orderBy);
        }

    }
}
