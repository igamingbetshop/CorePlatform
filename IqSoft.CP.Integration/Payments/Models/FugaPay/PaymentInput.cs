using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FugaPay
{
    public class PaymentInput
    {
        public string OrderID { get; set; }
        public string ClientUserName { get; set; }
        public string ClientMail { get; set; }
        public string Desc { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantSecretCode { get; set; }
        public string MerchantPublicKey { get; set; }
        public string SuccessUrl { get; set; }
        public string FailUrl { get; set; }
        public string RedirectUrl { get; set; }
     //   public int Timeout { get; set; }
        public string Channel { get; set; }
        public string BankCode { get; set; }
        public decimal Amount { get; set; }
        public OptModel Opt { get; set; }
        public string IbanOwner { get; set; }
        public string IbanNumber { get; set; }
    }

    public class OptModel
    {
        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "client_username")]
        public string ClientUsername { get; set; }

        [JsonProperty(PropertyName = "client_name")]
        public string ClientName { get; set; }

        [JsonProperty(PropertyName = "client_surname")]
        public string ClientSurname { get; set; }

        [JsonProperty(PropertyName = "client_email")]
        public string ClientEmail { get; set; }

        [JsonProperty(PropertyName = "client_birthday")]
        public string ClientBirthday { get; set; }

        [JsonProperty(PropertyName = "client_identityno")]
        public string ClientIdentityNo { get; set; }

        [JsonProperty(PropertyName = "client_sender_phone")]
        public string ClientSenderPhone { get; set; }

        [JsonProperty(PropertyName = "transaction_date")]
        public string TransactionDate { get; set; }

        [JsonProperty(PropertyName = "ipaddress")]
        public string IpAddress { get; set; }
    }
}
