using IqSoft.CP.Common.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.CommonCore.Models.WebSiteModels
{
    public class ApiNews
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
        public DateTime StartDate { get; set; }
    }
}
