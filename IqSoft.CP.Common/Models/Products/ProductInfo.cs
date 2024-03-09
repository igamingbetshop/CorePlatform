namespace IqSoft.CP.Common.Models.Products
{
    public class ProductInfo
    {
        public string Name { get; set; }
        public decimal RTP { get; set; }
        public string Volatility { get; set; }
        public decimal Profit { get; set; }
        public decimal Turnover { get; set; }
        public bool IsFavorite { get; set; }
        public string Tags { get; set; }
    }
}
