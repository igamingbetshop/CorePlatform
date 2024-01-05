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
    
    public partial class ClientSession
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientSession()
        {
            this.ClientLogs = new HashSet<ClientLog>();
            this.ClientSession1 = new HashSet<ClientSession>();
            this.Clients = new HashSet<Client>();
        }
    
        public long Id { get; set; }
        public int ClientId { get; set; }
        public string LanguageId { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public string Token { get; set; }
        public int ProductId { get; set; }
        public int DeviceType { get; set; }
        public System.DateTime StartTime { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public int State { get; set; }
        public string CurrentPage { get; set; }
        public Nullable<long> ParentId { get; set; }
        public string ExternalToken { get; set; }
        public string Source { get; set; }
        public Nullable<int> LogoutType { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<long> AccountId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientLog> ClientLogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSession> ClientSession1 { get; set; }
        public virtual ClientSession ClientSession2 { get; set; }
        public virtual Language Language { get; set; }
        public virtual Product Product { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Clients { get; set; }
        public virtual Client Client { get; set; }
        public virtual Account Account { get; set; }
    }
}
