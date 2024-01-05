using System;

namespace IqSoft.CP.AdminWebApi.Filters.Bets
{
    public class ApiFilterBetShopBet : ApiFilterBase
    {
        public int? PartnerId { get; set; }        

        public DateTime BetDateFrom { get; set; }

        public DateTime BetDateBefore { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation CashierIds { get; set; }

        public ApiFiltersOperation CashDeskIds { get; set; }

        public ApiFiltersOperation BetShopIds { get; set; }

        public ApiFiltersOperation BetShopNames { get; set; }

        public int? BetShopGroupId { get; set; }

        public ApiFiltersOperation BetShopGroupNames { get; set; }

        public ApiFiltersOperation ProductIds { get; set; }

        public ApiFiltersOperation ProductNames { get; set; }

        public ApiFiltersOperation ProviderIds { get; set; }

        public ApiFiltersOperation ProviderNames { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation RoundIds { get; set; }

        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation BetTypes { get; set; }

        public ApiFiltersOperation PossibleWins { get; set; }

        public ApiFiltersOperation BetAmounts { get; set; }

        public ApiFiltersOperation WinAmounts { get; set; }

        public ApiFiltersOperation Barcodes { get; set; }

        public ApiFiltersOperation TicketNumbers { get; set; }

        public ApiFiltersOperation BetDates { get; set; }
    }
}