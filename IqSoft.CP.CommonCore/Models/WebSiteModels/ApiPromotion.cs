using IqSoft.CP.Common.Models.AdminModels;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiPromotion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string ImageName { get; set; }
        public ApiSetting Segments { get; set; }
        public int Order { get; set; }
        public ApiSetting Languages { get; set; }
        public string StyleType { get; set; }
        public List<int> Visibility { get; set; }
    }
}
