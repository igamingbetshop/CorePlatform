﻿namespace IqSoft.CP.DAL.Models.Job
{
    public class ClientProductBet
    {
        public int ClientId { get; set; }

        public string CurrencyId { get; set; }
        
        public int ProductId { get; set; }
        
        public decimal Amount { get; set; }
        
        public decimal Percent { get; set; } 
    }
}
