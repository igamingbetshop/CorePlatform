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
    
    public partial class ActionLog
    {
        public long Id { get; set; }
        public int ActionId { get; set; }
        public string Domain { get; set; }
        public string Source { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public Nullable<long> SessionId { get; set; }
        public string Page { get; set; }
        public Nullable<long> ObjectId { get; set; }
        public Nullable<int> ObjectTypeId { get; set; }
        public string Language { get; set; }
        public Nullable<int> ResultCode { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long Date { get; set; }
    
        public virtual ActionLog ActionLog1 { get; set; }
        public virtual ActionLog ActionLog2 { get; set; }
        public virtual UserSession UserSession { get; set; }
    }
}