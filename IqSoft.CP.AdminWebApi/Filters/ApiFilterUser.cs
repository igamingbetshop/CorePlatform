namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterUser: ApiFilterBase
    {
        public int? Id { get; set; }
        public string AgentIdentifier { get; set; }
        public int? ParentId { get; set; }
        public int? Level { get; set; }
        public int? Type { get; set; }
        public int? State { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public bool? WithClients { get; set; }
        public bool? IsFromSuspend { get; set; }
        public string CurrencyId { get; set; }
        public bool WithDownlines { get; set; }

        public int? PartnerId { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation FirstNames { get; set; }

        public ApiFiltersOperation LastNames { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation Emails { get; set; }

        public ApiFiltersOperation Genders { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation LanguageIds { get; set; }

        public ApiFiltersOperation UserStates { get; set; }

        public ApiFiltersOperation UserTypes { get; set; }

        public ApiFiltersOperation UserRoles { get; set; }
    }
}