using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPartnerCurrenciesOutput : ApiResponseBase
    {
        public List<string> Currencies { get; set; }
    }
}