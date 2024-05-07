namespace IqSoft.CP.ProductGateway.Models.Endorphina
{
    public class WinInput : BaseInput
    {
        public long? amount { get; set; }
        public long? bonusWin { get; set; }
        public string betSessionId { get; set; }
        public long? betTransactionId { get; set; }
        public string date { get; set; }
        public int? gameId { get; set; }
        public string id { get; set; }
        public string progressive { get; set; }
        public string progressiveDesc { get; set; }
        public string promoId { get; set; }
        public string promoName { get; set; }
        public string state { get; set; }
    }
}