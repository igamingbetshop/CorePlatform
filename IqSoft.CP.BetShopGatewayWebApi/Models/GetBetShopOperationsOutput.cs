using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetBetShopOperationsOutput
    {
        public List<BetShopOperation> Operations { get; set; }
    }

    public class BetShopOperation
    {
        public long Id { get; set; }

        public long Barcode { get; set; }

        public int ClientId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public string ClientFirstName { get; set; }

        public string ClientLastName { get; set; }

        public string UserName { get; set; }

        public string DocumentNumber { get; set; }

        public string ClientEmail { get; set; }

        public int Type { get; set; }

        public System.DateTime CreationTime { get; set; }

        public System.DateTime LastUpdateTime { get; set; }
    }
}