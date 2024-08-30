using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Mpesa
{
    public class PayBillInput
    {
        public string TransactionType { get; set; }
        public string TransID { get; set; }
        public string TransTime { get; set; }
        public decimal TransAmount { get; set; }
        public string BusinessShortCode { get; set; }
        public string BillRefNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrgAccountBalance { get; set; }
        public string ThirdPartyTransID { get; set; }
        [JsonProperty(PropertyName = "MSISDN")]
        public string MobileNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }
}