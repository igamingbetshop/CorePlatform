using System;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllProductSetting
    {
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public string NickName { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public decimal? Rating { get; set; }
        public int? OpenMode { get; set; }
        public int SubproviderId { get; set; }
        public int? CategoryId { get; set; }
        public bool? HasDemo { get; set; }
    }
}
