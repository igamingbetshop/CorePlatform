using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiProduct
    {
        public int Id { get; set; }

        public long TranslationId { get; set; }

        public int? GameProviderId { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public string Description { get; set; }

        public int? ParentId { get; set; }

        public string ExternalId { get; set; }

        public int State { get; set; }

        public string MobileImageUrl { get; set; }
        
        public string WebImageUrl { get; set; }
        
        public string BackgroundImageUrl { get; set; }
    }
}
