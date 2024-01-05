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
    
    public partial class UserSession
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserSession()
        {
            this.ActionLogs = new HashSet<ActionLog>();
            this.CashDesks = new HashSet<CashDesk>();
            this.Currencies = new HashSet<Currency>();
            this.CurrencyRates = new HashSet<CurrencyRate>();
            this.ObjectChangeHistories = new HashSet<ObjectChangeHistory>();
            this.Partners = new HashSet<Partner>();
            this.PaymentRequestHistories = new HashSet<PaymentRequestHistory>();
            this.Users = new HashSet<User>();
            this.ClientMessages = new HashSet<ClientMessage>();
            this.PartnerPaymentSettings = new HashSet<PartnerPaymentSetting>();
            this.Notes = new HashSet<Note>();
            this.BetShops = new HashSet<BetShop>();
            this.BetShopGroups = new HashSet<BetShopGroup>();
            this.TranslationEntries = new HashSet<TranslationEntry>();
        }
    
        public long Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public string LanguageId { get; set; }
        public string Ip { get; set; }
        public string Token { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> CashDeskId { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public int State { get; set; }
        public Nullable<int> ProjectTypeId { get; set; }
        public Nullable<long> ParentId { get; set; }
        public Nullable<int> LogoutType { get; set; }
        public Nullable<int> AffiliateId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActionLog> ActionLogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CashDesk> CashDesks { get; set; }
        public virtual CashDesk CashDesk { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Currency> Currencies { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CurrencyRate> CurrencyRates { get; set; }
        public virtual Language Language { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ObjectChangeHistory> ObjectChangeHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Partner> Partners { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentRequestHistory> PaymentRequestHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
        public virtual User User { get; set; }
        public virtual Affiliate Affiliate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientMessage> ClientMessages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PartnerPaymentSetting> PartnerPaymentSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Note> Notes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BetShop> BetShops { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BetShopGroup> BetShopGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TranslationEntry> TranslationEntries { get; set; }
    }
}
