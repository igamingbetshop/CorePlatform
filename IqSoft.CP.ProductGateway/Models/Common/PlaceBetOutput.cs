using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.Common
{
    public class PlaceBetOutput
    {
        public int CashierId;
        public List<BetOutput> Bets { get; set; }
    }
}