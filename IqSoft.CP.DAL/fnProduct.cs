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
    
    public partial class fnProduct
    {
        public int Id { get; set; }
        public long TranslationId { get; set; }
        public Nullable<int> GameProviderId { get; set; }
        public Nullable<int> PaymentSystemId { get; set; }
        public int Level { get; set; }
        public string NickName { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string ExternalId { get; set; }
        public int State { get; set; }
        public bool IsForDesktop { get; set; }
        public bool IsForMobile { get; set; }
        public bool HasDemo { get; set; }
        public Nullable<int> SubproviderId { get; set; }
        public string WebImageUrl { get; set; }
        public string MobileImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public Nullable<bool> FreeSpinSupport { get; set; }
        public string Jackpot { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public Nullable<decimal> RTP { get; set; }
        public string Lines { get; set; }
        public string BetValues { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string GameProviderName { get; set; }
        public string SubproviderName { get; set; }
        public int IsLeaf { get; set; }
        public int IsLastProductGroup { get; set; }
    }
}