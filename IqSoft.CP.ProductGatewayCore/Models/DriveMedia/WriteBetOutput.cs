﻿using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.DriveMedia
{
    public class WriteBetOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "login")]
        public string[] Login { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public long[] Balance { get; set; }

        [JsonProperty(PropertyName = "operationId")]
        public string OperationId { get; set; }
    }
}