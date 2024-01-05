using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ClientMessageModel : ApiResponseBase
    {
        public long Id { get; set; }

        public int PartnerId { get; set; }

        public long? ParentId { get; set; }

        public int? ClientId { get; set; }

        public string Message { get; set; }

        public string Subject { get; set; }

        public int Type { get; set; }

        public int State { get; set; }

        public long SessionId { get; set; }

        public DateTime CreationTime { get; set; }

        public double TimeZone { get; set; }
    }
}