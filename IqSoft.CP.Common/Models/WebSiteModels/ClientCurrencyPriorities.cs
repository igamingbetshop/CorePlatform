using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ClientCurrencyPriorities : ApiResponseBase
    {
        public int ClientId { get; set; }
        public List<CurrencyPriority> Priorities { get; set; }
    }

    public class CurrencyPriority
    {
        public string CurrencyId { get; set; }
        public int Priority { get; set; }
    }
}