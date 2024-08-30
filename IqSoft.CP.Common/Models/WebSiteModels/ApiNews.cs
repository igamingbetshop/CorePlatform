using IqSoft.CP.Common.Models.AdminModels;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiNews
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string ImageName { get; set; }
        public ApiSetting Segments { get; set; }
        public ApiSetting Languages { get; set; }
        public int Order { get; set; }
        public int? ParentId { get; set; }
        public string StyleType { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime FinishDate { get; set; }
    }
}
