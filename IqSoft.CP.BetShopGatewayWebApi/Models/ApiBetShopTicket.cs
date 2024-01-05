using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiBetShopTicket : ApiResponseBase
    {
        public long Id { get; set; }

        public long DocumentId { get; set; }

        public string ExternalId { get; set; }

        public int GameId { get; set; }

        public long BarCode { get; set; }

        public int NumberOfPrints { get; set; }

        public DateTime? LastPrintTime { get; set; }

        public DateTime CreationTime { get; set; }
    }
}