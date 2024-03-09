using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashDesksBalanceOutput
    {
        public List<CashDeskBalanceOutput> CashDeskBalances { get; set; }
    }

    public class CashDeskBalanceOutput
    {
        public int CashDeskId { get; set; }
        public decimal Balance { get; set; }
    }
}
