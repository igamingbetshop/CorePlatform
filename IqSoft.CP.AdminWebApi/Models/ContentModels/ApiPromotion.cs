using IqSoft.CP.Common.Models.AdminModels;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiPromotion
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public string ImageName { get; set; }
        public string ImageData { get; set; }
        public string ImageDataMedium { get; set; }
        public string ImageDataSmall { get; set; }
        public int State { get; set; }
        public int EnvironmentTypeId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime FinishDate { get; set; }
        public ApiSetting Segments { get; set; }
        public ApiSetting Languages { get; set; }
        public int Order { get; set; }
        public int? ParentId { get; set; }
        public string StyleType { get; set; }
		public string SiteUrl { get; set; }
        public int? DeviceType { get; set; }
        public List<int> Visibility { get; set; }
    }
}