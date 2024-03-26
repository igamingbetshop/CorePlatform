using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterInternetGame : FilterBase<fnInternetGame>
    {
        public int? PartnerId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public FiltersOperation ProductIds { get; set; }

        public FiltersOperation ProductNames { get; set; }

        public FiltersOperation ProviderIds { get; set; }
        
        public FiltersOperation SubproviderIds { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation BetAmounts { get; set; }

        public FiltersOperation WinAmounts { get; set; }
        public FiltersOperation OriginalBetAmounts { get; set; }
        public FiltersOperation OriginalWinAmounts { get; set; }

        public FiltersOperation GGRs { get; set; }

        public FiltersOperation BetCounts { get; set; }

        public override void CreateQuery(ref IQueryable<fnInternetGame> objects, bool orderBy, bool orderByDate = false)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, ProviderIds, "ProviderId");
            FilterByValue(ref objects, SubproviderIds, "SubproviderId");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, BetAmounts, "BetAmount");
            FilterByValue(ref objects, WinAmounts, "WinAmount");
            FilterByValue(ref objects, OriginalBetAmounts, "OriginalBetAmount");
            FilterByValue(ref objects, OriginalWinAmounts, "OriginalWinAmount");
            FilterByValue(ref objects, GGRs, "GGR");
            FilterByValue(ref objects, BetCounts, "BetCount");

            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }

        public IQueryable<fnInternetGame> FilterObjects(IQueryable<fnInternetGame> internetBets, bool ordering)
        {
            CreateQuery(ref internetBets, ordering);
            return internetBets;
        }
    }
}
