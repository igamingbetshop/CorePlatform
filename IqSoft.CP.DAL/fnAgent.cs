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
    
    public partial class fnAgent
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LoginIp { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }
        public int Gender { get; set; }
        public string LanguageId { get; set; }
        public string UserName { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public string Email { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string Path { get; set; }
        public Nullable<bool> AllowOutright { get; set; }
        public Nullable<bool> AllowDoubleCommission { get; set; }
        public Nullable<bool> AllowAutoPT { get; set; }
        public Nullable<int> ParentState { get; set; }
        public string CalculationPeriod { get; set; }
        public string Phone { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string NickName { get; set; }
        public string MobileNumber { get; set; }
        public Nullable<int> Level { get; set; }
        public int Type { get; set; }
        public Nullable<int> ClientCount { get; set; }
        public Nullable<int> DirectClientCount { get; set; }
        public Nullable<decimal> Balance { get; set; }
    }
}
