using IqSoft.CP.Common.Models.AdminModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiBanner
    {    
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int EnvironmentTypeId { get; set; }
        public string Body { get; set; }
        public string Head { get; set; }
        public string Link { get; set; }
        public int Order { get; set; }
        public string NickName { get; set; }
        public string ImageData { get; set; }
        public string Image { get; set; }
        public List<string> ImageSizes { get; set; }
        public bool IsEnabled { get; set; }
        public bool ShowDescription { get; set; }
        public bool ShowRegistration { get; set; }
        public bool ShowLogin { get; set; }
        public int Type { get; set; }
        public List<int> Visibility { get; set; }
        public string FragmentName { get; set; }
        public string ImageSize { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ApiSetting Segments { get; set; }
    }
}