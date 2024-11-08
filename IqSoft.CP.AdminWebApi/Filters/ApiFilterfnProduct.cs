﻿using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterfnProduct : ApiFilterBase
    {
        public int? ParentId { get; set; }
        public int? ProductId { get; set; }
        public int? BonusId { get; set; }
        public string Pattern { get; set; }
        public bool? IsProviderActive { get; set; }
        public bool? IsForMobile { get; set; }
        public bool? IsForDesktop { get; set; }
        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation Names { get; set; }

        public ApiFiltersOperation Descriptions { get; set; }

        public ApiFiltersOperation ExternalIds { get; set; }

        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation GameProviderIds { get; set; }

        public ApiFiltersOperation SubProviderIds { get; set; }
        
        public ApiFiltersOperation FreeSpinSupports { get; set; }
        
        public ApiFiltersOperation Jackpots { get; set; }
        public ApiFiltersOperation RTPs { get; set; }
        ///bonus specific filters
        public ApiFiltersOperation Percents { get; set; }
        public ApiFiltersOperation ProductIds { get; set; }
        public ApiFiltersOperation Counts { get; set; }
        public ApiFiltersOperation Lines { get; set; }
        public ApiFiltersOperation Coins { get; set; }
        public ApiFiltersOperation CoinValues { get; set; }
        public ApiFiltersOperation BetValues { get; set; }

    }
}