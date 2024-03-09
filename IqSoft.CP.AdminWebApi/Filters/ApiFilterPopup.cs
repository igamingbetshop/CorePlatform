namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterPopup : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation NickNames { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation Types { get; set; }
        public ApiFiltersOperation Orders { get; set; }
        public ApiFiltersOperation Pages { get; set; }
        public ApiFiltersOperation StartDates { get; set; }
        public ApiFiltersOperation FinishDates { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation LastUpdateTimes { get; set; }
    }
}