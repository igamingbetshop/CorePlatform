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
    
    public partial class fnClientMessage
    {
        public int PartnerId { get; set; }
        public long MessageId { get; set; }
        public Nullable<int> Id { get; set; }
        public string UserName { get; set; }
        public string MobileOrEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        public Nullable<int> Status { get; set; }
        public System.DateTime CreationTime { get; set; }
    }
}
