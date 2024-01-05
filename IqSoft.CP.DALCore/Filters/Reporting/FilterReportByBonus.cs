using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
   public  class FilterReportByBonus : FilterBase<fnClientBonus>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation BonusIds { get; set; }
        public FiltersOperation BonusNames { get; set; }
        public FiltersOperation BonusTypes { get; set; }
        public FiltersOperation BonusStatuses { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation CategoryIds { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation BonusPrizes { get; set; }
        public FiltersOperation TurnoverAmountLefts { get; set; }
        public FiltersOperation RemainingCredits { get; set; }
        public FiltersOperation FinalAmounts { get; set; }
        public FiltersOperation ClientBonusStatuses { get; set; }
        public FiltersOperation AwardingTimes { get; set; }
        public FiltersOperation CalculationTimes { get; set; }
        public FiltersOperation ValidUntils { get; set; }

        protected override IQueryable<fnClientBonus> CreateQuery(IQueryable<fnClientBonus> objects, Func<IQueryable<fnClientBonus>, IOrderedQueryable<fnClientBonus>> orderBy = null)
        {
            var fDate = (long)FromDate.Year * 100000000 + (long)FromDate.Month * 1000000 +
                   FromDate.Day * 10000 + FromDate.Hour * 100 + FromDate.Minute;
            var tDate = (long)ToDate.Year * 100000000 + (long)ToDate.Month * 1000000 +
                              ToDate.Day * 10000 + ToDate.Hour * 100 + ToDate.Minute;
            objects = objects.Where(x => x.CreationDate >= fDate);
            objects = objects.Where(x => x.CreationDate < tDate);

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, BonusIds, "BonusId");
            FilterByValue(ref objects, BonusNames, "Name");
            FilterByValue(ref objects, BonusTypes, "BonusType");
            FilterByValue(ref objects, BonusStatuses, "BonusStatus");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, CategoryIds, "CategoryId");
            FilterByValue(ref objects, BonusPrizes, "BonusPrize");
            FilterByValue(ref objects, TurnoverAmountLefts, "TurnoverAmountLeft");
            FilterByValue(ref objects, RemainingCredits, "RemainingCredit");
            FilterByValue(ref objects, FinalAmounts, "FinalAmount");
            FilterByValue(ref objects, ClientBonusStatuses, "ClientBonusStatus");
            FilterByValue(ref objects, AwardingTimes, "AwardingTime");
            FilterByValue(ref objects, ValidUntils, "ValidUntil");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnClientBonus> FilterObjects(IQueryable<fnClientBonus> objects, Func<IQueryable<fnClientBonus>, IOrderedQueryable<fnClientBonus>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
        public long SelectedObjectsCount(IQueryable<fnClientBonus> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }
    }
}