namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class MyAffiliateClientActivityModel : ClientActivityModel
    {
        public decimal ConvertedBonusAmount { get; set; }
        public decimal CreditCorrectionOnClient { get; set; }
        public decimal DebitCorrectionOnClient { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal BonusBalance { get; set; }
        public decimal ChargeBack { get; set; }
        public decimal NGR { get; set; }
    }
}
