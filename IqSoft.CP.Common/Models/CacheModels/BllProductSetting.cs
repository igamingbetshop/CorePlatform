using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllProductSetting
    {
        public int I { get; set; } //ProductId
        public string N { get; set; } //Name
        public string NN { get; set; } //NickName
        public string SN { get; set; } //SubproviderName
        public decimal? R { get; set; } //Rating
        public int? OM { get; set; } //OpenMode
        public int SI { get; set; } //SubproviderId
        public string C { get; set; } //Categories
        public List<int> CI { get; set; } //CategoryIds
        public bool? HD { get; set; } //HasDemo
    }
}
