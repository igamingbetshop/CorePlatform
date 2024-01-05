namespace IqSoft.CP.ProductGateway.Models.OutcomeBet
{
    public class BalanceInput
    {
        public int CasinoId { get; set; }
        public string PlayerId { get; set; }
        public ContextModel Context { get; set; }
    }
}