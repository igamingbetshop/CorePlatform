using System.Collections.Generic;
namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetBalanceOutput : ApiResponseBase
    {
        public string CurrencyId { get; set; }

        public decimal AvailableBalance { get; set; }

        public IEnumerable<ApiAccountBalance> Balances { get; set; }
    }

    public class ApiAccountBalance
    {
        public long Id { get; set; }

        public int TypeId { get; set; }

        public decimal Balance { get; set; }

        public int? BetShopId { get; set; }

        public int? PaymentSystemId { get; set; }
    }
}