using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPartnerMenuItemsOutput : ApiResponseBase
    {
        public List<WebSiteMenuModels> MenuList { get; set; }
    }

    public class WebSiteMenuModels
    {
        public string Type { get; set; }
        public bool IsEnabled { get; set; }
        public List<WebSiteMenuItemsModel> Items { get; set; }
    }

    public class WebSiteMenuItemsModel
    {
        public int Id { get; set; }
        public Dictionary<string, string> Titles { get; set; }
        public string TitleNickName { get; set; }
        public int Type { get; set; }
        public int Order { get; set; }
        public string Href { get; set; }
    }
}