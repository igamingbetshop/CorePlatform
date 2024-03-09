using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class ApiPartnerPaymentSetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public string PaymentSystemName { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public decimal? Commission { get; set; }
        public decimal? FixedFee { get; set; }
        public decimal? ApplyPercentAmount { get; set; }
        public int Type { get; set; }
        public string Info { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool? AllowMultipleClientsPerPaymentInfo { get; set; }
        public bool? AllowMultiplePaymentInfoes { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<int> Countries { get; set; }
        public List<int> OSTypes { get; set; }
        public int Priority { get; set; }
        public int? OpenMode { get; set; }
    }
}