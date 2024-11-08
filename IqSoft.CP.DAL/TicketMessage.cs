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
    
    public partial class TicketMessage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TicketMessage()
        {
            this.TicketMessageStates = new HashSet<TicketMessageState>();
        }
    
        public long Id { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long TicketId { get; set; }
        public Nullable<int> UserId { get; set; }
        public long CreationDate { get; set; }
    
        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketMessageState> TicketMessageStates { get; set; }
    }
}
