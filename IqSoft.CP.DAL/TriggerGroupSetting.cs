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
    
    public partial class TriggerGroupSetting
    {
        public int Id { get; set; }
        public int SettingId { get; set; }
        public int GroupId { get; set; }
        public int Order { get; set; }
    
        public virtual TriggerGroup TriggerGroup { get; set; }
        public virtual TriggerSetting TriggerSetting { get; set; }
    }
}