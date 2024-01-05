using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterInternetBet : ApiFilterBase
    {
        public int ClientId { get; set; }

        public string CurrencyId { get; set; }

        public int? Status { get; set; }

        public int? GroupId { get; set; }

        public List<int> ProductIds { get; set; }

        public DateTime CreatedFrom { get; set; }

        public DateTime CreatedBefore { get; set; }
    }
}