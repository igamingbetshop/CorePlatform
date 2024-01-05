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
    
    public partial class Jackpot
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Jackpot()
        {
            this.JackpotSettings = new HashSet<JackpotSetting>();
            this.JobTriggers = new HashSet<JobTrigger>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> PartnerId { get; set; }
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public string WinAmount { get; set; }
        public System.DateTime FinishTime { get; set; }
        public Nullable<int> WinnerId { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JackpotSetting> JackpotSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JobTrigger> JobTriggers { get; set; }
    }
}
