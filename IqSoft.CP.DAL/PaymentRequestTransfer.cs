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
    
    public partial class PaymentRequestTransfer
    {
        public long Id { get; set; }
        public int PaymentSystemId { get; set; }
        public long PaymentRequestId { get; set; }
        public int State { get; set; }
        public System.DateTime LastSendDate { get; set; }
        public string Response { get; set; }
        public int RetryCount { get; set; }
    
        public virtual PaymentSystem PaymentSystem { get; set; }
        public virtual PaymentRequest PaymentRequest { get; set; }
    }
}
