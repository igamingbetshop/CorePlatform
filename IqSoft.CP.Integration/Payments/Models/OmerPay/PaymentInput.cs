using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.OmerPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "journal")]
        public string Journal { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "address_1")]
        public string Address { get; set; }

        //[JsonProperty(PropertyName = "address_2")]
        //public string SecondAddress { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }


        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }

        [JsonProperty(PropertyName = "date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }


        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(PropertyName = "cardholder_name")]
        public string CardHolderName { get; set; }

        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }


        [JsonProperty(PropertyName = "card_exp_month")]
        public int CardExpMonth { get; set; }

        [JsonProperty(PropertyName = "card_exp_year")]
        public int CardExpYear { get; set; }

        [JsonProperty(PropertyName = "approval_return_url")]
        public string ApprovalReturnUrl { get; set; }

        [JsonProperty(PropertyName = "decline_return_url")]
        public string DeclineReturnUrl { get; set; }

        [JsonProperty(PropertyName = "cancel_return_url")]
        public string CancelReturnUrl { get; set; }

        [JsonProperty(PropertyName = "notification_url")]
        public string NotificationUrl { get; set; }

    }

    public class Notification
    {
        [JsonProperty(PropertyName = "server_date")]
        public DateTime ServerDate { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "journal")]
        public string Journal { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string UuId { get; set; }
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "usd_amount")]
        public decimal UsdAmount { get; set; }

        [JsonProperty(PropertyName = "exchange_rate")]
        public decimal ExchangeRate { get; set; }

        [JsonProperty(PropertyName = "descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }

    public class RefundRequest
    {
        [JsonProperty(PropertyName = "uuid")]
        public string UuId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

    }

    public class TransactionStatusRequest
    {
        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }
    }


}
