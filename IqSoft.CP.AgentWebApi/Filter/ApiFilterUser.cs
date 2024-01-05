namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterUser: ApiFilterBase
    {
        public int? PartnerId { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation FirstNames { get; set; }

        public ApiFiltersOperation LastNames { get; set; }

        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation NickNames { get; set; }

        public ApiFiltersOperation Emails { get; set; }
        public ApiFiltersOperation MobileNumbers { get; set; }

        public ApiFiltersOperation Genders { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation LanguageIds { get; set; }

        public ApiFiltersOperation UserStates { get; set; }

        public ApiFiltersOperation UserTypes { get; set; }

        public ApiFiltersOperation Balances { get; set; }
    }
}