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
    
    public partial class CurrencyGroup
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public Nullable<int> BonusId { get; set; }
    
        public virtual Currency Currency { get; set; }
        public virtual Bonu Bonu { get; set; }
    }
}
