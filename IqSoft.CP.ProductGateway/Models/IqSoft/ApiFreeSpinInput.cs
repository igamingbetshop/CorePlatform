using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.IqSoft
{
    public class ApiFreeSpinInput
    {
        public int ClientId { get; set; }
        public int SpinCount { get; set; }
        public string ValidUntil { get; set; }
        public int ProductId { get; set; }
        public string ApiToken { get; set; }
        public int? Lines { get; set; }
        public int? Coins { get; set; }
        public int? CoinValue { get; set; }
        public int? BetValueLevel { get; set; }
    }
}