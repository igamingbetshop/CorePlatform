using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class GetSessionInfoOutput : ResponseBase
    {
        public decimal GivenCredit { get; set; }

        public decimal AvailableCredit { get; set; }

        public decimal CashBalance { get; set; }

        public decimal OutstandingBalance { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime PasswordExpiryDate { get; set; }
        
        public DateTime LastTransactionDate { get; set; }
    }
}
