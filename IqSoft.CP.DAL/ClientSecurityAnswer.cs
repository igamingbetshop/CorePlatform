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
    
    public partial class ClientSecurityAnswer
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int SecurityQuestionId { get; set; }
        public string Answer { get; set; }
    
        public virtual SecurityQuestion SecurityQuestion { get; set; }
        public virtual Client Client { get; set; }
    }
}
