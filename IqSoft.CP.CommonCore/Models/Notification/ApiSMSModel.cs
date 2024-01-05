namespace IqSoft.CP.Common.Models
{
    public class ApiSMSModel
    {
        public int PartnerId { get; set; }
        public string Recipient { get; set; }
        public string MessegeText { get; set; }
        public long MessageId { get; set; }
    }
}
