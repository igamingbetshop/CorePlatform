using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Corefy
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "data")]
        public InputDataModel Data { get; set; }
    }
    public class InputDataModel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public InputAttribute Attributes { get; set; }
    }

    public class InputAttribute
    {
        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }

        [JsonProperty(PropertyName = "reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal? Amount { get; set; }

        [JsonProperty(PropertyName = "test_mode")]
        public bool TestMode { get; set; }

        [JsonProperty(PropertyName = "return_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty(PropertyName = "callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty(PropertyName = "service_fields")]
        public ServiceFields ServiceFields { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public CustomerModel Customer { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public InputFields Fields { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Metadata MetadataFields { get; set; }

    }

    public class ServiceFields
    {
        [JsonProperty(PropertyName = "identification_number")]
        public string IdentificationNumber { get; set; }
    }

    public class CustomerModel
    {
        [JsonProperty(PropertyName = "reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonProperty(PropertyName = "address")]
        public AddressModel Address { get; set; }
    }

    public class AddressModel
    {
        [JsonProperty(PropertyName = "full_address")]
        public string FullAddress { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "post_code")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
    }

    public class InputFields
    {
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "account_number")]
        public string AccountNumber { get; set; }

        [JsonProperty(PropertyName = "document_id")]
        public string DocumentId { get; set; }

        [JsonProperty(PropertyName = "user_name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "wallet_id")]
        public string MaldoCrypto { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "branch_code")]
        public string BranchCode { get; set; }

        [JsonProperty(PropertyName = "bank_branch_code")]
        public string BankBranchCode { get; set; }

        [JsonProperty(PropertyName = "beneficiary_full_name")]
        public string BeneficiaryFullName { get; set; }

        [JsonProperty(PropertyName = "beneficiary_account_number")]
        public string BeneficiaryAccountNumber { get; set; }

        [JsonProperty(PropertyName = "cpf_number")]
        public string CpfNumber { get; set; }

        [JsonProperty(PropertyName = "beneficiary_name")]
        public string BeneficiaryName { get; set; }

        [JsonProperty(PropertyName = "beneficiary_lastname")]
        public string BeneficiaryLastname { get; set; }

        [JsonProperty(PropertyName = "pix_key")]
        public string PixKey { get; set; }

        [JsonProperty(PropertyName = "account_type")]
        public string AccountType { get; set; }

    }

    public class Metadata
    {
        [JsonProperty(PropertyName = "coin")]
        public string CryptoCoin { get; set; }

        [JsonProperty(PropertyName = "bank_code")]
        public string BankCode { get; set; }

    }
}
