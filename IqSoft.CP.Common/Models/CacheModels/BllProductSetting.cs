using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
        public string Categories
        {
            set
            {
                CategoryIds = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<List<int>>(value) : null;
            }
        }
        public List<int> CategoryIds        { get; set; }
        
        public bool? HasDemo { get; set; }
    }
}
