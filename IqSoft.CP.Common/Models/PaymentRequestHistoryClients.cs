using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models
{
    public class PaymentRequestHistoryClients
    {
        public decimal? TotalWithdrawAmount { get; set; }
        public decimal? TotalDepositAmount { get; set; }
        public DateTime? LastDepositDate { get; set; }
        public int? CountOfDepositLastWeek { get; set; }
        public int? CountOfDeposits { get; set; }
        public DateTime? FirstDepositDate { get; set; }
        public DateTime? LastWithdrawDate { get; set; }

    }
}
