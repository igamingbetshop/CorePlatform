//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DataWarehouse
{
    using System;
    using System.Collections.Generic;
    
    public partial class DuplicatedClient
    {
        public DuplicatedClient()
        {
            this.ClientMatchHistories = new HashSet<ClientMatchHistory>();
        }
    
        public long Id { get; set; }
        public int ClientId { get; set; }
        public int MatchedClientId { get; set; }
        public System.DateTime MatchDate { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual Client Client1 { get; set; }
        public virtual ICollection<ClientMatchHistory> ClientMatchHistories { get; set; }
    }
}