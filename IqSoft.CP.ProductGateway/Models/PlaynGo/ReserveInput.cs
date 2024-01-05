using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.PlaynGo.Input
{
    public class Reserve : Balance
    {
        public string transactionId { get; set; }
        public decimal real { get; set; }
        public string gameSessionId { get; set; }
        public string roundId { get; set; }
        public string freegameExternalId { get; set; }
        public decimal actualValue { get; set; }
        public List<Jackpot> jackpots { get; set; }
    }

    public class Jackpot
    {
        public string id { get; set; }
        public string loss { get; set; }
        public string win { get; set; }
    }

}