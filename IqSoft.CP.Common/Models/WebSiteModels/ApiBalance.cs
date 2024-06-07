using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiBalance
    {
        public decimal AvailableBalance { get; set; }
        public List<ApiAccount> Balances { get; set; }
    }
}
