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
    
    public partial class fnPromotion
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public string ImageName { get; set; }
        public int State { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime FinishDate { get; set; }
        public int Order { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string StyleType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}