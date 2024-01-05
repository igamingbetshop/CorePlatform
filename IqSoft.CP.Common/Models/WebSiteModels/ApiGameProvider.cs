using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiGameProvider
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public int? SessionExpireTime { get; set; }

        public string GameLaunchUrl { get; set; }
    }
}
