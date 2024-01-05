namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterfnProduct : ApiFilterBase
    {
        public int? ParentId { get; set; }
        public int? ProductId { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation Names { get; set; }

        public ApiFiltersOperation Descriptions { get; set; }

        public ApiFiltersOperation ExternalIds { get; set; }

        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation GameProviderIds { get; set; }

        public ApiFiltersOperation SubProviderIds { get; set; }

        public ApiFiltersOperation IsForDesktops { get; set; }

        public ApiFiltersOperation IsForMobiles { get; set; }
        
        public ApiFiltersOperation FreeSpinSupports { get; set; }
        
        public ApiFiltersOperation Jackpots { get; set; }

        public ApiFiltersOperation RTPs { get; set; }
    }
}