using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterBetShopReconing : ApiFilterBase
    {
        public List<ApiFiltersOperationType> Ids { get; set; }

        public List<ApiFiltersOperationType> UserIds { get; set; }

        public List<ApiFiltersOperationType> Currencies { get; set; }

        public List<ApiFiltersOperationType> BetShopIds { get; set; }

        public List<ApiFiltersOperationType> BetShopNames { get; set; }

        public List<ApiFiltersOperationType> BetShopAvailiableBalances { get; set; }

        public List<ApiFiltersOperationType> Amounts { get; set; }

        public List<ApiFiltersOperationType> CreationTimes { get; set; }
    }
}