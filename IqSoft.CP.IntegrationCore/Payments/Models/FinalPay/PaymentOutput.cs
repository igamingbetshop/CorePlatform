namespace IqSoft.CP.Integration.Payments.Models.FinalPay
{
   public class PaymentOutput
    {
        public string Checksum { get; set; }
        public Data Data { get; set; }
        public string State { get; set; }  
        public string Msg { get; set; }
    }
    public class Data
    {
        public string Redirect_url { get; set; }
        public string Trans_ref { get; set; }
    }
}
