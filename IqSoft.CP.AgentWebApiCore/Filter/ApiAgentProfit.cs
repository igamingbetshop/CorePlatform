using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiAgentProfit
    {
        public int AgentId { get; set; }       

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public decimal Amount { get; set; }

        public int? FromAgentId { get; set; }

        public DateTime CreationDate { get; set; }
    }
}