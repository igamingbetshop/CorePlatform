using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class FnPartnerProductSettingModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int State { get; set; }
        public decimal? Rating { get; set; }
        public decimal Percent { get; set; }
        public int PartnerId { get; set; }        
        public int? ProductGameProviderId { get; set; }        
        public string GameProviderName { get; set; }
		public List<int> CategoryIds { get; set; }
		public decimal? RTP { get; set; }
		public string ExternalId { get; set; }
		public int? Volatility { get; set; }
        public int? SubproviderId { get; set; }        
        public int? OpenMode { get; set; }
        public bool IsForMobile { get; set; }
        public bool IsForDesktop { get; set; }
        public bool? HasDemo { get; set; }
        public string Jackpot { get; set; }
        public string MobileImageUrl { get; set; }
        public string WebImageUrl { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
    }
}