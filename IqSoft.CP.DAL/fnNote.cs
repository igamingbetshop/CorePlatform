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
    
    public partial class fnNote
    {
        public long Id { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public long SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public Nullable<int> CommentTemplateId { get; set; }
        public string CreatorFirstName { get; set; }
        public string CreatorLastName { get; set; }
    }
}