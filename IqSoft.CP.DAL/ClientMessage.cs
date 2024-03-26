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
    
    public partial class ClientMessage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientMessage()
        {
            this.ClientMessageStates = new HashSet<ClientMessageState>();
        }
    
        public long Id { get; set; }
        public Nullable<long> ParentId { get; set; }
        public int PartnerId { get; set; }
        public Nullable<int> ClientId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public Nullable<long> SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public Nullable<int> EmailId { get; set; }
        public string MobileOrEmail { get; set; }
        public Nullable<int> AffiliateId { get; set; }
    
        public virtual UserSession UserSession { get; set; }
        public virtual Affiliate Affiliate { get; set; }
        public virtual Email Email { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientMessageState> ClientMessageStates { get; set; }
        public virtual Client Client { get; set; }
    }
}
