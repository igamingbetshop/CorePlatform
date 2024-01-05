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
    
    public partial class JobTrigger
    {
        public long Id { get; set; }
        public int ClientId { get; set; }
        public int Type { get; set; }
        public Nullable<int> SegmentId { get; set; }
        public Nullable<int> JackpotId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<long> PaymentRequestId { get; set; }
    
        public virtual Jackpot Jackpot { get; set; }
        public virtual Segment Segment { get; set; }
        public virtual Client Client { get; set; }
        public virtual PaymentRequest PaymentRequest { get; set; }
    }
}
