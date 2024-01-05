using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiCashierSession : ApiResponseBase
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public string LanguageId { get; set; }

        public string Ip { get; set; }

        public string Token { get; set; }

        public int? ProductId { get; set; }

        public int? CashDeskId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int State { get; set; }

        public int? ProjectTypeId { get; set; }

        public long? ParentId { get; set; }
    }
}