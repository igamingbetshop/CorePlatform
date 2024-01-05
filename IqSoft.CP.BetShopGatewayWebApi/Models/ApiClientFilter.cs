﻿namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiClientFilter : ApiFilterBase
	{
        public int? ClientId { get; set; }

        public int CashDeskId { get; set; }
        
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string DocumentNumber { get; set; }
        
        public string Email { get; set; }
        public string Info { get; set; }
    }
}