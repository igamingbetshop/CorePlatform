using IqSoft.CP.Common.Attributes;
using IqSoft.CP.Common.Models.AdminModels;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.AdminWebApi.Models.ProductModels
{
    public class FnProductModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public int? GameProviderId { get; set; }
        public string GameProviderName { get; set; }
        public int? SubproviderId { get; set; }
        public string SubproviderName { get; set; }
        [NotExcelProperty]
        public int State { get; set; }
        [JsonProperty(PropertyName = "State"), JsonIgnore]
        public string StateName { get; set; }
        public bool IsForDesktop { get; set; }
		public bool IsForMobile { get; set; }
		public bool HasDemo { get; set; }
		public bool? FreeSpinSupport { get; set; }
        public string WebImageUrl { get; set; }
        public string WebImage { get; set; }
        public string MobileImageUrl { get; set; }
        public string MobileImage { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string BackgroundImage { get; set; }
        public int? CategoryId { get; set; }
        public decimal? RTP { get; set; }
        public string Lines { get; set; }
        public string BetValues { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        [NotExcelProperty]
        public int NewId { get; set; }

        [NotExcelProperty]
        public long TranslationId { get; set; }

        [NotExcelProperty]
        public int? PaymentSystemId { get; set; }
        [NotExcelProperty]
        public int Level { get; set; }

        [NotExcelProperty]
        public string Jackpot { get; set; }

        [NotExcelProperty]
        public ApiSetting Countries { get; set; }
    }
}