using IqSoft.CP.DAL.Filters;

namespace IqSoft.CP.DAL
{
    public class BonusCondition
    {
        public FiltersOperation GroupingType { get; set; }
        public FiltersOperation Event { get; set; }
        public FiltersOperation Market { get; set; }
        public FiltersOperation Class { get; set; }
        public FiltersOperation Type { get; set; }
        public FiltersOperation Selection { get; set; }
        public FiltersOperation Sport { get; set; }
        public FiltersOperation BetType { get; set; }
        public FiltersOperation InPlay { get; set; }
        public FiltersOperation MarketSort { get; set; }
        public FiltersOperation NumberOfLegs { get; set; }
        public FiltersOperation NumberOfLines { get; set; }
        public FiltersOperation Price { get; set; }
        public FiltersOperation PricePerSelection { get; set; }
        public FiltersOperation Stake { get; set; }
    }
}
