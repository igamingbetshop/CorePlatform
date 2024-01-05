using Newtonsoft.Json;

namespace IqSoft.CP.DAL.Models.Report
{
    public class WithdrawPaymentSystemReportItem
    {
        [JsonProperty(PropertyName = "Payment Name")]
        public string PaymentSystemName { get; set; }

        [JsonProperty(PropertyName = "Total Withdrawals Amount")]
        public decimal TotalAmount { get; set; }
    }
    public class DepositPaymentSystemReportItem
    {
        [JsonProperty(PropertyName = "Payment Name")]
        public string PaymentSystemName { get; set; }

        [JsonProperty(PropertyName = "Total Deposits Amount")]
        public decimal TotalAmount { get; set; }
    }
}