﻿namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashDeskCurrentBalanceOutput : ApiResponseBase
	{
		public decimal Balance { get; set; }
		public decimal TerminalBalance { get; set; }
        public decimal CashDeskBalance { get; set; }

        public decimal CurrentLimit { get; set; }
    }
}