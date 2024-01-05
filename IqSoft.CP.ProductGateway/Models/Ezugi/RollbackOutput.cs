﻿using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class RollbackOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "roundId")]
        public long RoundId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }
    }
}