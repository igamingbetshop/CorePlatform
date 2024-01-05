using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.Kiron
{
    public class DebitInput : BaseInput
    {
        public string PlayerID { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string BetManTransactionID { get; set; }
        public string RoundID { get; set; }
        public List<int> GameIds { get; set; }
    }
}