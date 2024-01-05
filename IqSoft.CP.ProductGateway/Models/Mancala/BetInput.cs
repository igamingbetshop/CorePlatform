namespace IqSoft.CP.ProductGateway.Models.Mancala
{
    public class BetInput : BaseInput
    {
        public decimal Amount { get; set; }
        public string TransactionGuid { get; set; }
        public string RoundGuid { get; set; }
        public string BonusTransaction { get; set; }
        public string ExternalBonusId { get; set; }
    }
}