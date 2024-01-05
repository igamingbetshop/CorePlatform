using IqSoft.CP.Common.Models.WebSiteModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.CommonCore.Models.WebSiteModels
{
    public class ApiPromotionGroup
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public int Order { get; set; }
        public string StyleType { get; set; }
        public List<ApiPromotion> Promotions { get; set; }
    }
}
