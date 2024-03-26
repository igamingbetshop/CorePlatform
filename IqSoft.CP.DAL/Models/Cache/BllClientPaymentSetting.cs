using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClientPaymentSetting
    {
        public int PaymentSystemId { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
    }
}
