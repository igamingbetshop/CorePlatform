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
    
    public partial class PartnerPaymentSegmentSetting
    {
        public int Id { get; set; }
        public int PartnerPaymentSettingId { get; set; }
        public int SegmentId { get; set; }
        public int Type { get; set; }
    
        public virtual PartnerPaymentSetting PartnerPaymentSetting { get; set; }
        public virtual Segment Segment { get; set; }
    }
}
