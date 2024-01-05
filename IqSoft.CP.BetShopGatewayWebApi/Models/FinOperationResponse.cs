﻿namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class FinOperationResponse : ApiResponseBase
    {
        public decimal CashierBalance { get; set; }
        public decimal ClientBalance { get; set; }
        public decimal CurrentLimit { get; set; }
        public string CurrencyId { get; set; }
    }
}