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
    
    public partial class fnCashDesks
    {
        public int Id { get; set; }
        public int BetShopId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public string EncryptIv { get; set; }
        public string EncryptSalt { get; set; }
        public string MacAddress { get; set; }
        public int Type { get; set; }
        public Nullable<int> Restrictions { get; set; }
        public decimal Balance { get; set; }
        public Nullable<int> AccountTypeId { get; set; }
    }
}