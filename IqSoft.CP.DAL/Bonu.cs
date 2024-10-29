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
    
    public partial class Bonu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bonu()
        {
            this.Bonus1 = new HashSet<Bonu>();
            this.BonusCountrySettings = new HashSet<BonusCountrySetting>();
            this.BonusCurrencySettings = new HashSet<BonusCurrencySetting>();
            this.BonusLanguageSettings = new HashSet<BonusLanguageSetting>();
            this.BonusPaymentSystemSettings = new HashSet<BonusPaymentSystemSetting>();
            this.BonusSegmentSettings = new HashSet<BonusSegmentSetting>();
            this.TriggerGroups = new HashSet<TriggerGroup>();
            this.AmountCurrencySettings = new HashSet<AmountCurrencySetting>();
            this.ClientBonusTriggers = new HashSet<ClientBonusTrigger>();
            this.ClientBonus = new HashSet<ClientBonu>();
            this.ClientBonus1 = new HashSet<ClientBonu>();
            this.BonusProducts = new HashSet<BonusProduct>();
            this.UserNotifications = new HashSet<UserNotification>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PartnerId { get; set; }
        public Nullable<int> FinalAccountTypeId { get; set; }
        public int Status { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime FinishTime { get; set; }
        public System.DateTime LastExecutionTime { get; set; }
        public int Period { get; set; }
        public int Type { get; set; }
        public string Info { get; set; }
        public Nullable<int> TurnoverCount { get; set; }
        public Nullable<decimal> MinAmount { get; set; }
        public Nullable<decimal> MaxAmount { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<long> TranslationId { get; set; }
        public Nullable<int> Priority { get; set; }
        public Nullable<int> WinAccountTypeId { get; set; }
        public Nullable<int> ValidForAwarding { get; set; }
        public Nullable<int> ValidForSpending { get; set; }
        public Nullable<int> ReusingMaxCount { get; set; }
        public Nullable<bool> ResetOnWithdraw { get; set; }
        public Nullable<System.DateTime> CreationTime { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public Nullable<bool> AllowSplit { get; set; }
        public Nullable<bool> RefundRollbacked { get; set; }
        public string Condition { get; set; }
        public Nullable<decimal> Percent { get; set; }
        public Nullable<decimal> MaxGranted { get; set; }
        public Nullable<decimal> TotalGranted { get; set; }
        public Nullable<int> MaxReceiversCount { get; set; }
        public Nullable<int> TotalReceiversCount { get; set; }
        public Nullable<int> LinkedBonusId { get; set; }
        public Nullable<decimal> AutoApproveMaxAmount { get; set; }
        public Nullable<bool> FreezeBonusBalance { get; set; }
        public Nullable<int> Regularity { get; set; }
        public Nullable<int> DayOfWeek { get; set; }
        public Nullable<int> ReusingMaxCountInPeriod { get; set; }
        public string Color { get; set; }
        public Nullable<int> WageringSource { get; set; }
    
        public virtual AccountType AccountType { get; set; }
        public virtual AccountType AccountType1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bonu> Bonus1 { get; set; }
        public virtual Bonu Bonu1 { get; set; }
        public virtual Partner Partner { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BonusCountrySetting> BonusCountrySettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BonusCurrencySetting> BonusCurrencySettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BonusLanguageSetting> BonusLanguageSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BonusPaymentSystemSetting> BonusPaymentSystemSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BonusSegmentSetting> BonusSegmentSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TriggerGroup> TriggerGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AmountCurrencySetting> AmountCurrencySettings { get; set; }
        public virtual Translation Translation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientBonusTrigger> ClientBonusTriggers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientBonu> ClientBonus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientBonu> ClientBonus1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BonusProduct> BonusProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserNotification> UserNotifications { get; set; }
    }
}
