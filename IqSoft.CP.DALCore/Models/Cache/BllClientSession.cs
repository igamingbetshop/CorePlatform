using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClientSession
    {
        public long Id { get; set; }

        public int ClientId { get; set; }

        public string LanguageId { get; set; }
        
        public string Ip { get; set; }
        
        public string Country { get; set; }
        
        public string Token { get; set; }
        
        public int ProductId { get; set; }
        
        public int DeviceType { get; set; }
        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public DateTime LastUpdateTime { get; set; }
        
        public int State { get; set; }
        
        public string CurrentPage { get; set; }
        
        public long? ParentId { get; set; }

		public string CurrencyId { get; set; }

        public string ExternalToken { get; set; }
        public int? LogoutType { get; set; }
    }
}
