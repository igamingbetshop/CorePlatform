using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashiersBalanceIntput : RequestBase
    {
        public DateTime BalanceDate { get; set; }
        public List<CashDeskBalanceInput> CashDesks { get; set; }
    }

    public class CashDeskBalanceInput
    {
        public int CashDeskId { get; set; }
        public string CurrencyId { get; set; }
    }
}