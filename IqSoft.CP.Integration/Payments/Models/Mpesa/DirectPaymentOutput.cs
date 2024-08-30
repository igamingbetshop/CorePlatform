namespace IqSoft.CP.Integration.Payments.Models.Mpesa
{
    public class DirectPaymentOutput
    {
        public string OriginatorConversationID { get; set; }
        public string ConversationID { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
    }
}
