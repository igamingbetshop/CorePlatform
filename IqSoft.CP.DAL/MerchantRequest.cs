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
    
    public partial class MerchantRequest
    {
        public long Id { get; set; }
        public string RequestUrl { get; set; }
        public string Content { get; set; }
        public string Response { get; set; }
        public int Status { get; set; }
        public int RetryCount { get; set; }
    }
}
