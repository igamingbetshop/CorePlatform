//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DataWarehouse
{
    using System;
    
    public partial class fnClientSession
    {
        public long Id { get; set; }
        public int ClientId { get; set; }
        public string LanguageId { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public string Token { get; set; }
        public int ProductId { get; set; }
        public int DeviceType { get; set; }
        public System.DateTime StartTime { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public int State { get; set; }
        public string CurrentPage { get; set; }
        public Nullable<long> ParentId { get; set; }
        public string ExternalToken { get; set; }
        public string Source { get; set; }
        public Nullable<int> LogoutType { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<long> AccountId { get; set; }
        public int PartnerId { get; set; }
    }
}