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
    
    public partial class ComplimentaryPointRate
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
    
        public virtual Partner Partner { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual Product Product { get; set; }
    }
}
