using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllPartnerPaymentSetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public long SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int PaymentSystemPriority { get; set; }
        public decimal Commission { get; set; }
        public decimal FixedFee { get; set; }
        public int Type { get; set; }
        public string Info{ get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool? AllowMultipleClientsPerPaymentInfo { get; set; }
        public bool? AllowMultiplePaymentInfoes { get; set; }
        public int? OpenMode { get; set; }
        public List<int> Countries { get; set; }
        public string OSTypesString { get; set; }
        public List<int> OSTypes
        {
            get
            {
                if (!string.IsNullOrEmpty(OSTypesString))
                    return OSTypesString.Split(',').Select(Int32.Parse).ToList();
                return null; 
            }
        }
        public List<BllCurrencyRate> CurrencyRates { get; set; }
    }
}
