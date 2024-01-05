namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class PaymentFormRequest
    {
        public int PartnerId { get; set; }

        public string ClientIdentifier { get; set; }

        public decimal Amount { get; set; }

        public string Info { get; set; }

        public string PaymentForm { get; set; }

        public string ImageName { get; set; }

        public int Type { get; set; }
    }
}