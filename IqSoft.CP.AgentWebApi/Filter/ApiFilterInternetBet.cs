using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterInternetBet : ApiFilterBase
    {
        public int? PartnerId { get; set; }

        public int? AgentId { get; set; }

        public DateTime BetDateFrom { get; set; }

        public DateTime BetDateBefore { get; set; }

        public ApiFiltersOperation BetDocumentIds { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation Names { get; set; }
        public ApiFiltersOperation ClientFirstNames { get; set; }
        public ApiFiltersOperation ClientLastNames { get; set; }

        public ApiFiltersOperation ClientUserNames { get; set; }

        public ApiFiltersOperation Categories { get; set; }

        public ApiFiltersOperation ProductIds { get; set; }

        public ApiFiltersOperation ProductNames { get; set; }

        public ApiFiltersOperation ProviderNames { get; set; }

        public ApiFiltersOperation CurrencyIds { get; set; }

        public ApiFiltersOperation RoundIds { get; set; }

        public ApiFiltersOperation DeviceTypes { get; set; }

        public ApiFiltersOperation ClientIps { get; set; }

        public ApiFiltersOperation Countries { get; set; }

        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation BetTypes { get; set; }

        public ApiFiltersOperation PossibleWins { get; set; }

        public ApiFiltersOperation BetAmounts { get; set; }

        public ApiFiltersOperation WinAmounts { get; set; }

        public ApiFiltersOperation BetDates { get; set; }

        public ApiFiltersOperation BonusIds { get; set; }


        public ApiFiltersOperation GGRs { get; set; }

        public ApiFiltersOperation Balances { get; set; }

        public ApiFiltersOperation TotalBetsCounts { get; set; }

        public ApiFiltersOperation TotalBetsAmounts { get; set; }

        public ApiFiltersOperation TotalWinsAmounts { get; set; }

        public ApiFiltersOperation MaxBetAmounts { get; set; }

        public ApiFiltersOperation TotalDepositsCounts { get; set; }

        public ApiFiltersOperation TotalDepositsAmounts { get; set; }

        public ApiFiltersOperation TotalWithdrawalsCounts { get; set; }

        public ApiFiltersOperation TotalWithdrawalsAmounts { get; set; }
    }
}