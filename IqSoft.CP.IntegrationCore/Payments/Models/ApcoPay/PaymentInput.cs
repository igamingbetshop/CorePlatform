
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.ApcoPay
{
    public class PaymentInput
    {
        public Transaction Transaction { get; set; }
    }

    public class Transaction
    {
        [XmlAttribute(AttributeName = "hash")]
        public string Hash { get; set; }

        public string ProfileID { get; set; }

        public int ActionType { get; set; }

        public decimal Value { get; set; }

        public string Curr { get; set; }

        public string Lang { get; set; }

        public string ORef { get; set; }

        public string RedirectionURL { get; set; }

        public string UDF1 { get; set; }

        public string UDF2 { get; set; }

        public string UDF3 { get; set; }

        public string MobileNo { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string RegCountry { get; set; }

        public string DOB { get; set; }

        public string RegName { get; set; }

        public string CIP { get; set; }

        public string NoCardList { get; set; }

        public string RecurringPayments { get; set; }

        //public string TEST { get; set; }

        public string ClientAcc { get; set; }

        public string MainAcquirer { get; set; }

        public string ForcePayment { get; set; }

        public string ForceBank { get; set; }

        public FastPayType FastPay { get; set; }

        public string status_url { get; set; }

        public string FailedRedirectionURL { get; set; }

        public string return_pspid { get; set; }

        public string HideSSLLogo { get; set; }

        public AntiFraudType AntiFraud { get; set; }

        public string NeoSurfEmail { get; set; }

        public string NationalID { get; set; }

        public string payoutType { get; set; }

        public string BeneficiaryID { get; set; }

        public string BankName { get; set; }

        public string BankBranch { get; set; }

        public string BankAccount { get; set; }

        public string IBAN { get; set; }

        public string BankCode { get; set; }

        public string AccountType { get; set; }

		public string NewCard1Try { get; set; }

        public string PspID { get; set; }

       // public string payoutType { get; set; }


    }

    public class FastPayType
    {
        public string ListAllCards { get; set; }

        public string NewCardOnFail { get; set; }

        public string PromptCVV { get; set; }

        public string PromptExpiry { get; set; }
    }

    public class AntiFraudType
    {
        public string Provider { get; set; }
    }
}