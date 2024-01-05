﻿namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashDeskInfoOutput : ApiResponseBase
    {
        public int CashDeskId { get; set; }

        public decimal Balance { get; set; }

        public string CurrencyId { get; set; }

        public decimal CurrentLimit { get; set; }
    }
}