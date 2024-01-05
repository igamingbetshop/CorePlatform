namespace IqSoft.CP.ProductGateway.Models.Endorphina
{
    public class WinInput
    {
        public long amount { get; set; }

        public string betSessionId { get; set; }

        public long betTransactionId { get; set; }

        public string currency { get; set; }

        public string date { get; set; }

        public string game { get; set; }

        public int gameId { get; set; }

        public long? id { get; set; }

        public string player { get; set; }

        public string progressive { get; set; }

        public string progressiveDesc { get; set; }

        public string token { get; set; }

        public string sign { get; set; }
    }
}