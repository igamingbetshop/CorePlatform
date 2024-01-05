namespace IqSoft.CP.AgentWebApi.Filters
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
    }
}