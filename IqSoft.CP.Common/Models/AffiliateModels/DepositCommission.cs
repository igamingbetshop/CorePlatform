using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
    public class DepositCommission
    {
        public string CurrencyId { get; set; }
        public decimal? Percent { get; set; }
        public decimal? UpToAmount { get; set; }
        public int? DepositCount { get; set; }
    }
}
