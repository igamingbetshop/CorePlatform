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
    
    public partial class TriggerGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TriggerGroup()
        {
            this.TriggerGroupSettings = new HashSet<TriggerGroupSetting>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int BonusId { get; set; }
        public int Type { get; set; }
        public int Priority { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TriggerGroupSetting> TriggerGroupSettings { get; set; }
        public virtual Bonu Bonu { get; set; }
    }
}
