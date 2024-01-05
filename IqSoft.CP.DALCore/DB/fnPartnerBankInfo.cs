using System;

namespace IqSoft.CP.DAL
{
    public partial class fnPartnerBankInfo
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int? PaymentSystemId { get; set; }
        public string BankName { get; set; }
        public string NickName { get; set; }
        public string BankCode { get; set; }
        public string OwnerName { get; set; }
        public string BranchName { get; set; }
        public string IBAN { get; set; }
        public string AccountNumber { get; set; }
        public string CurrencyId { get; set; }
        public long TranslationId { get; set; }
        public bool Active { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int Type { get; set; }
        public int Order { get; set; }
    }
}
