//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DAL
{
    using System;
    
    public partial class fnPartnerPaymentSetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public decimal Commission { get; set; }
        public decimal FixedFee { get; set; }
        public Nullable<decimal> ApplyPercentAmount { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public long SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public int PaymentSystemPriority { get; set; }
        public int Type { get; set; }
        public string OSTypes { get; set; }
        public string Info { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public Nullable<bool> AllowMultipleClientsPerPaymentInfo { get; set; }
        public Nullable<bool> AllowMultiplePaymentInfoes { get; set; }
        public string PaymentSystemName { get; set; }
        public int PaymenSystemType { get; set; }
        public Nullable<int> ContentType { get; set; }
        public string ImageExtension { get; set; }
    }
}
