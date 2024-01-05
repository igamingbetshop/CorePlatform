using IqSoft.CP.Common.Models.AdminModels;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiBannerOutput
    {
        public string Body { get; set; }
        public string Head { get; set; }
        public string Link { get; set; }
        public int Order { get; set; }
        public string Image { get; set; }
        public List<string> ImageSizes { get; set; }
        public bool ShowDescription { get; set; }
        public bool ShowRegistration { get; set; }
        public bool ShowLogin { get; set; }

        public List<int> Visibility { get; set; }
        public int? ButtonType { get; set; }
        public ApiSetting Segments { get; set; }
    }
}