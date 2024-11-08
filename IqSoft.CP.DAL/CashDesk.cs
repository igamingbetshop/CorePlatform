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
    
    public partial class CashDesk
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CashDesk()
        {
            this.CashDeskShifts = new HashSet<CashDeskShift>();
            this.Documents = new HashSet<Document>();
            this.UserSessions = new HashSet<UserSession>();
            this.PaymentRequests = new HashSet<PaymentRequest>();
        }
    
        public int Id { get; set; }
        public int BetShopId { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public string MacAddress { get; set; }
        public string EncryptPassword { get; set; }
        public string EncryptSalt { get; set; }
        public string EncryptIv { get; set; }
        public long SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public int CurrentCashierId { get; set; }
        public Nullable<int> CurrentShiftNumber { get; set; }
        public int Type { get; set; }
        public Nullable<int> Restrictions { get; set; }
    
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CashDeskShift> CashDeskShifts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Document> Documents { get; set; }
        public virtual UserSession UserSession { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserSession> UserSessions { get; set; }
        public virtual BetShop BetShop { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; }
    }
}
