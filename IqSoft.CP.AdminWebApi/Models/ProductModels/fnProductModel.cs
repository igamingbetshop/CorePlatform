using IqSoft.CP.Common.Models.AdminModels;
using System;

namespace IqSoft.CP.AdminWebApi.Models.ProductModels
{
    public class FnProductModel
    {
        public int Id { get; set; }
        public int NewId { get; set; }
        public long TranslationId { get; set; }
        public int? GameProviderId { get; set; }
        public int? PaymentSystemId { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string GameProviderName { get; set; }
        public int IsLeaf { get; set; }
        public int IsLastProductGroup { get; set; }
        public int State { get; set; }
		public bool IsForDesktop { get; set; }
		public bool IsForMobile { get; set; }
		public bool HasDemo { get; set; }
		public string Jackpot { get; set; }
		public bool? FreeSpinSupport { get; set; }
		public int? SubproviderId { get; set; }
        public string WebImageUrl { get; set; }
        public string MobileImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string SubproviderName { get; set; }
        public int? CategoryId { get; set; }
        public decimal? RTP { get; set; }
        public string Lines { get; set; }
        public string BetValues { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public ApiSetting Countries { get; set; }
    }
}