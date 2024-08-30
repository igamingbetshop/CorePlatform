using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiCurrency
    {
        public string Id { get; set; }

        public decimal CurrentRate { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }
    }
}
