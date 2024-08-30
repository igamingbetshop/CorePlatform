using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterBanner : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? ShowDescription { get; set; }
        public bool? ShowRegistration { get; set; }
        public bool? ShowLogin { get; set; }
        public int? Visibility { get; set; }
        public ApiFiltersOperation StartDates { get; set; }
        public ApiFiltersOperation EndDates { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation Orders { get; set; }
        public ApiFiltersOperation NickNames { get; set; }
        public ApiFiltersOperation Images { get; set; }
        public ApiFiltersOperation Types { get; set; }
        public ApiFiltersOperation FragmentNames { get; set; }
        public ApiFiltersOperation Bodies { get; set; }
        public ApiFiltersOperation Heads { get; set; }
    }
}