namespace IqSoft.CP.ProductGateway.Models.BetSolutions
{
    public class BetInput : BaseInput
    {
        public long Amount { get; set; }
        public string TransactionId { get; set; }
        public int? BetTypeId { get; set; }
        public int? WinTypeId { get; set; }
        public  string RoundId { get; set; }
        public string CampaignId { get; set; }
        public string CampaignName { get; set; }
    }
}