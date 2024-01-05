using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterPartnerProductsMapping : ApiFilterBase
    {
        public FiltersOperation Ids { get; set; }

        public FiltersOperation Names { get; set; }

        public FiltersOperation Descriptions { get; set; }

        public FiltersOperation GameProviderIds { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation PPIds { get; set; }

        public FiltersOperation PPPartnerIds { get; set; }

        public FiltersOperation PPProductNames { get; set; }

        public FiltersOperation PPProductDescriptions { get; set; }

        public FiltersOperation PPGameProviderIds { get; set; }

        public FiltersOperation PPStates { get; set; }

        public FiltersOperation PPPercents { get; set; }
    }
}
