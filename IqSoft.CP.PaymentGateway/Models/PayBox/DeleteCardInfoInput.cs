namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    public class DeleteCardInfoInput
    {
        public string pg_merchant_id { get; set; }
        public long pg_user_id { get; set; }
        public long pg_card_id { get; set; }
        public string pg_salt { get; set; }
        public string pg_sig { get; set; }
    }
}