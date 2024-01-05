namespace IqSoft.CP.Integration.Payments.Models.ApcoPay
{
   public  class PaymentOutput
    {
        public string Result { get; set; } 
        public string ErrorMsg { get; set; } 
        public string BaseURL { get; set; }
        public string Token { get; set; }
    }
}
