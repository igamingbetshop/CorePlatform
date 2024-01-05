using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllMerchant
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string MerchantKey { get; set; }
        public string MerchantUrl { get; set; }
    }
}
