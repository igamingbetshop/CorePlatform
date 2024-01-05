using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiBalance
    {
        public decimal AvailableBalance { get; set; }
        public List<ApiAccount> Balances { get; set; }
    }
}
