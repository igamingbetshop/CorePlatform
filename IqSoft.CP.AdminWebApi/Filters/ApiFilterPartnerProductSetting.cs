using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterPartnerProductSetting : ApiFilterBase
    {
        public int PartnerId { get; set; }
        public int? ProviderId { get; set; }
        public int? CategoryIds { get; set; }
        public bool? HasImages { get; set; }
        public bool? IsForMobile { get; set; }
        public bool? IsForDesktop { get; set; }
        public bool? HasDemo { get; set; }
        public ApiFiltersOperation ProductIsLeaf { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation ProductIds { get; set; }
        public ApiFiltersOperation ProductDescriptions { get; set; }
        public ApiFiltersOperation ProductNames { get; set; }
        public ApiFiltersOperation GameProviderIds { get; set; }
        public ApiFiltersOperation ProductExternalIds { get; set; }
        public ApiFiltersOperation SubProviderIds { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation OpenModes { get; set; }
        public ApiFiltersOperation RTPs { get; set; }
        public ApiFiltersOperation ExternalIds { get; set; }
        public ApiFiltersOperation Volatilities{ get; set; }
        public ApiFiltersOperation Ratings { get; set; }
        public ApiFiltersOperation Percents { get; set; }
        public ApiFiltersOperation Jackpots { get; set; }
    }
}