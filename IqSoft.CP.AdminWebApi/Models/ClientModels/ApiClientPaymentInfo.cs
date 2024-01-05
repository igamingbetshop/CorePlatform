using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientPaymentInfo
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public string CardholderName { get; set; }

        public string CardNumber { get; set; }

        public DateTime? CardExpireDate { get; set; }

        public string BankName { get; set; }

        public string Iban { get; set; }

        public string BankAccountNumber { get; set; }

        public string NickName { get; set; }

        public int Type { get; set; }

        public int? State { get; set; }

        public string WalletNumber { get; set; }

        public int? PaymentSystem { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}