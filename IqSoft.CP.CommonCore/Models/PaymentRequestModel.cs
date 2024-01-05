using IqSoft.CP.Common.Models.WebSiteModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models
{
    public class PaymentRequestModel
    {
        public long Id { get; set; }
        public int ClientId { get; set; }
        public int PartnerId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public string RecipientAccount { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public int Status { get; set; }
        public int? BetShopId { get; set; }
        public int PaymentSystemId { get; set; }
        public string Info { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int Type { get; set; }
        public Nullable<int> CashDeskId { get; set; }
        public string UserName { get; set; }
        public string FirtName { get; set; }
        public string LastName { get; set; }
        public int GroupId { get; set; }
        public string VerificationCode { get; set; }
        public string CashCode { get; set; }
        public string StatusName { get; set; }
        public string Url { get; set; }
        public string Comment { get; set; }
        public decimal? CommissionAmount { get; set; }
        public ApiBalance ApiBalance { get; set; }
        public bool SavePaymentDetails { get; set; }
    }
}
