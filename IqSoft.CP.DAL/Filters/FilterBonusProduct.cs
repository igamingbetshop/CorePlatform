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
        public FiltersOperation Lineses { get; set; }
        public FiltersOperation Coinses { get; set; }
        public FiltersOperation CoinValues { get; set; }
        public FiltersOperation BetValueLevels { get; set; }

        protected override IQueryable<BonusProduct> CreateQuery(IQueryable<BonusProduct> objects, Func<IQueryable<BonusProduct>, IOrderedQueryable<BonusProduct>> orderBy = null)
        {
            if (BonusId.HasValue)
                objects = objects.Where(x => x.BonusId == BonusId.Value);
            FilterByValue(ref objects, BonusIds, "BonusId");
            FilterByValue(ref objects, Percents, "Percent");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, Counts, "Count");
            FilterByValue(ref objects, Lineses, "Lines");
            FilterByValue(ref objects, Coinses, "Coins");
            FilterByValue(ref objects, CoinValues, "CoinValue");
            FilterByValue(ref objects, BetValueLevels, "BetValueLevel");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<BonusProduct> FilterObjects(IQueryable<BonusProduct> fnProducts, Func<IQueryable<BonusProduct>, IOrderedQueryable<BonusProduct>> orderBy = null)
        {
            return CreateQuery(fnProducts, orderBy);
        }

    }
}
