using System;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllGeolocationData
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public string LanguageId { get; set; }
        public string CurrencyId { get; set; }
    }
}
