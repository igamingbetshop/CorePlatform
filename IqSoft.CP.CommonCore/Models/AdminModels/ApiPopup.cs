using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiPopup
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }       
        public int? DeviceType { get; set; }       
        public string ImageName { get; set; }
        public int Order { get; set; }
        public string Page { get; set; }
        public string SiteUrl { get; set; }
        public List<int> ClientIds { get; set; }
    }

}
