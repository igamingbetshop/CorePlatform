using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiPartnerBankInfo
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string OwnerName { get; set; }
        public List<ApiClientPaymentInfo> Accounts { get; set; }
    }
}