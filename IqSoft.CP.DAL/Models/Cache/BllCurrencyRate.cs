using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllCurrencyRate
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
    }
}