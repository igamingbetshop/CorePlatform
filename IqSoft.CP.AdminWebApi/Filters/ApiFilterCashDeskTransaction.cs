using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterCashDeskTransaction : ApiFilterBase
    {
        public DateTime CreatedFrom { get; set; }

        public DateTime CreatedBefore { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation BetShopNames { get; set; }

        public ApiFiltersOperation CashierIds { get; set; }

        public ApiFiltersOperation CashDeskIds { get; set; }

        public ApiFiltersOperation BetShopIds { get; set; }

        public ApiFiltersOperation OperationTypeNames { get; set; }

        public ApiFiltersOperation Amounts { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation CreationTimes { get; set; }
    }
}