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
    
    public partial class ProductLimit
    {
        public long Id { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public int LimitTypeId { get; set; }
        public Nullable<decimal> MinLimit { get; set; }
        public Nullable<decimal> MaxLimit { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> RowState { get; set; }
    
        public virtual LimitType LimitType { get; set; }
        public virtual ObjectType ObjectType { get; set; }
        public virtual Product Product { get; set; }
    }
}
