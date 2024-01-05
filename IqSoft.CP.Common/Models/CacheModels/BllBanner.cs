using System;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllBanner
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int Type { get; set; }
        public string NickName { get; set; }
        public string Head { get; set; }
        public Nullable<long> HeadTranslationId { get; set; }
        public string Body { get; set; }
        public Nullable<long> BodyTranslationId { get; set; }
        public string Link { get; set; }
        public bool ShowDescription { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }
        public string Image { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> Visibility { get; set; }
        public string VisibilityInfo { get; set; }
        public int? ButtonType { get; set; }
        public BllSetting Segments { get; set; }
        public BllSetting Languages { get; set; }
    }
}
