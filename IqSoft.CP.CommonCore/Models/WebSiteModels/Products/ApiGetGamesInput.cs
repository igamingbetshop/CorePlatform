using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels.Products
{
    public class ApiGetGamesInput : ApiRequestBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool WithWidget { get; set; }
        public bool IsForMobile { get; set; }
        public int? CategoryId { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> ProviderIds { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public int? ClientId { get; set; }
        public string Pattern { get; set; }
    }
}
