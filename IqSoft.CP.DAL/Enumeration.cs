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
    
    public partial class Enumeration
    {
        public int Id { get; set; }
        public string EnumType { get; set; }
        public string NickName { get; set; }
        public int Value { get; set; }
        public long TranslationId { get; set; }
    
        public virtual Translation Translation { get; set; }
    }
}
