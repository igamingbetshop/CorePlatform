using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class ApiPartnerBankInfo
    {
        public int? Id { get; set; }
        public int PartnerId { get; set; }
        public int? PaymentSystemId { get; set; }
        public string BankName { get; set; }
        public string NickName { get; set; }
        public string BankCode { get; set; }
        public string BranchName { get; set; }
        public string OwnerName { get; set; }
        public string IBAN { get; set; }
        public string CurrencyId { get; set; }
        public bool Active { get; set; }
        public int Type { get; set; }
        public int Order { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public List<string> Accounts { get; set; }
    }
}