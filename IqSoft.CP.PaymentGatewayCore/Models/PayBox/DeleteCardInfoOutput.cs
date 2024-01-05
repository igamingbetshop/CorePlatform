namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    public class DeleteCardInfoOutput
    {
        public string pg_status { get; set; }
        public string pg_merchant_id { get; set; }
        public long pg_card_id { get; set; }
        public string pg_card_hash { get; set; }
        public string pg_salt { get; set; }
        public string deleted_at { get; set; }
    }
}