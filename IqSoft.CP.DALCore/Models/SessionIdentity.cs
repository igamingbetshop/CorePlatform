using System;

namespace IqSoft.CP.DAL.Models
{
    public class SessionIdentity
    {
        public int Id { get; set; }

        public string LoginIp { get; set; }

        public string LanguageId { get; set; }

        public string CurrencyId { get; set; }

        public double TimeZone { get; set; }

        public long ExternalOperationId { get; set; }

        public int PartnerId { get; set; }

        public long SessionId { get; set; }

        public string Token { get; set; }

        public int ProductId { get; set; }

        public int CashDeskId { get; set; }

        public string Country { get; set; }

        public string Domain { get; set; }

        public int DeviceType { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime ParentSessionStartTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public int State { get; set; }

        public string CurrentPage { get; set; }

        public long? ParentId { get; set; }

        public bool IsAdminUser { get; set; }
        public int? OddsType { get; set; }
        
        public object RequiredParameters { get; set; }
        public string Source { get; set; }
    }
}