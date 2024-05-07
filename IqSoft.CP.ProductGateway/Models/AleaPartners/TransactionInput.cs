using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.AleaPartners
{
    public class TransactionInput : BaseInput
    {
        [JsonProperty(PropertyName = "application")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "ticketId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "receivedDate")]
        public string ReceivedDate { get; set; }

        [JsonProperty(PropertyName = "accountType")]
        public string AccountType { get; set; }

        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; }

        [JsonProperty(PropertyName = "externalTransactionId")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "ccy")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "rollbackType")]
        public string RollbackType { get; set; }

        [JsonProperty(PropertyName = "ticketPromotions")]
        public string TicketPromotions { get; set; }

        [JsonProperty(PropertyName = "ticketDetails")]
        public string TicketDetails { get; set; }
    }

    public class TicketItem
    {
        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }
        public string Event { get; set; }
        public string Type { get; set; }

        [JsonProperty(PropertyName = "GameID")]
        public string GameId { get; set; }
        public string Value { get; set; }
        public string BetValue { get; set; }
        public decimal FutureCoefficient { get; set; }
        public decimal MaximumPayoutAfterTax { get; set; }

    }
}