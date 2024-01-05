using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiClientPaymentInfo
    {
        public int Id { get; set; }

        public string NickName { get; set; }

        public string ClientName { get; set; }

        public string OwnerName { get; set; }

        public string CardNumber { get; set; }

        public DateTime? CardExpireDate { get; set; }

        public string BankName { get; set; }

        public string Iban { get; set; }

        public string BankAccountNumber { get; set; }

        public string Type { get; set; }

        public string Code { get; set; }

        public string WalletNumber { get; set; }
    }
}
