using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllPaymentSystem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public int PeriodicityOfRequest { get; set; }
        public int PaymentRequestSendCount { get; set; }
        public int Type { get; set; }
        public long TranslationId { get; set; }
        public int ContentType { get; set; }
    }
}
