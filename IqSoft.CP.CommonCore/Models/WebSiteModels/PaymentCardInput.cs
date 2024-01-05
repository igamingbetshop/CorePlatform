namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class PaymentCardInput
    {
        public int ClientId { get; set; }

        public string Name { get; set; }

        public string CardNumber { get; set; }

        public System.DateTime ExpDate { get; set; }

        public int CardType { get; set; }

        public string BankName { get; set; }

        public string BankSwiftCode { get; set; }

        public string BankAccountNumber { get; set; }
    }
}