using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiAccount
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public string CurrencyId { get; set; }
        public decimal Balance { get; set; }
        public int? BetShopId { get; set; }
        public int? PaymentSystemId { get; set; }
    }
}
