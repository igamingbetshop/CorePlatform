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
    
    public partial class DuplicatedClientHistory
    {
        public int Id { get; set; }
        public long DuplicateId { get; set; }
        public string DuplicatedData { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public long CreationDate { get; set; }
    
        public virtual DuplicatedClient DuplicatedClient { get; set; }
    }
}
