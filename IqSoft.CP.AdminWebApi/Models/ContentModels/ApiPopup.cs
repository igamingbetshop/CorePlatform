using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiPopup
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int EnvironmentTypeId { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public string ImageName { get; set; }
        public string ImageData { get; set; }
        public string MobileImageName { get; set; }
        public string MobileImageData { get; set; }
        public long TranslationId { get; set; }
        public int Order { get; set; }
        public string Page { get; set; }
        public string SiteUrl { get; set; }
        public List<int> SegmentIds { get; set; }
        public List<int> ClientIds { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime FinishDate { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
    }
}
