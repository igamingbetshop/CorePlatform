using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Products
{
    public class ApiPartnerProductSettingInput
    {
        public int PartnerId { get; set; }
        public decimal? Percent { get; set; }
        public int? State { get; set; }
        public decimal? Rating { get; set; }
        public List<int> CategoryIds { get; set; }
        public decimal? RTP { get; set; }
        public int? Volatility { get; set; }
        public int? OpenMode { get; set; }
        public bool? HasDemo { get; set; }
        public List<int> ProductIds { get; set; }
    }
}