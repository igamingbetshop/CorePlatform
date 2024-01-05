namespace IqSoft.CP.Integration.Products.Models.IqSoft
{
    public partial class PartnerProductModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int PartnerId { get; set; }
        public decimal Percent { get; set; }
        public int State { get; set; }
        public decimal? Rating { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? OpenMode { get; set; }
        public bool? HasDemo { get; set; }
        public string ProductNickName { get; set; }
        public string ProductName { get; set; }
        public int? ProductGameProviderId { get; set; }
        public int? ProductParentId { get; set; }
        public int? PaymentSystemId { get; set; }
        public string GameProviderName { get; set; }
        public int? SubproviderId { get; set; }
        public bool IsForDesktop { get; set; }
        public bool IsForMobile { get; set; }
        public string Jackpot { get; set; }
        public string MobileImageUrl { get; set; }
        public string WebImageUrl { get; set; }
        public decimal? RTP { get; set; }
        public int? Volatility { get; set; }
    }
}
