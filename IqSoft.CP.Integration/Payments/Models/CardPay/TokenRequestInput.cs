namespace IqSoft.CP.Integration.Payments.Models.CardPay
{
    class TokenRequestInput
    {
        public string grant_type { get; set; }
        public int terminal_code { get; set; }
        public string password { get; set; }
    }
}
