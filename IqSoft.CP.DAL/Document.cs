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
    using System.Collections.Generic;
    
    public partial class Document
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Document()
        {
            this.Document1 = new HashSet<Document>();
            this.Transactions = new HashSet<Transaction>();
        }
    
        public long Id { get; set; }
        public string ExternalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public int OperationTypeId { get; set; }
        public Nullable<int> TypeId { get; set; }
        public Nullable<long> ParentId { get; set; }
        public Nullable<long> PaymentRequestId { get; set; }
        public string RoundId { get; set; }
        public string Info { get; set; }
        public Nullable<int> Creator { get; set; }
        public Nullable<int> CashDeskId { get; set; }
        public Nullable<int> PartnerPaymentSettingId { get; set; }
        public Nullable<int> PartnerProductId { get; set; }
        public Nullable<int> GameProviderId { get; set; }
        public Nullable<int> ClientId { get; set; }
        public Nullable<long> ExternalOperationId { get; set; }
        public Nullable<long> TicketNumber { get; set; }
        public string TicketInfo { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> DeviceTypeId { get; set; }
        public Nullable<decimal> PossibleWin { get; set; }
        public Nullable<long> SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<bool> HasNote { get; set; }
        public Nullable<long> Date { get; set; }
    
        public virtual CashDesk CashDesk { get; set; }
        public virtual Currency Currency { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Document> Document1 { get; set; }
        public virtual Document Document2 { get; set; }
        public virtual GameProvider GameProvider { get; set; }
        public virtual OperationType OperationType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual PartnerPaymentSetting PartnerPaymentSetting { get; set; }
        public virtual Product Product { get; set; }
        public virtual Client Client { get; set; }
    }
}