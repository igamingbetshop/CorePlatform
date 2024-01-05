using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
    public class FixedFeeCommission
    {
        public string CurrencyId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TotalDepositAmount { get; set; }
        public bool? RequireVerification { get; set; }
    }
}
