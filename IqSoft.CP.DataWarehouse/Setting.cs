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
    
    public partial class Setting
    {
        public int Id { get; set; }
        public Nullable<int> PartnerId { get; set; }
        public Nullable<int> GameProviderId { get; set; }
        public Nullable<int> PaymentSystemId { get; set; }
        public string Name { get; set; }
        public string StringValue { get; set; }
        public Nullable<System.DateTime> DateValue { get; set; }
        public Nullable<decimal> NumericValue { get; set; }
        public Nullable<int> NotificationServiceId { get; set; }
    }
}
