using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Spayz
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "result")]
        public ResultModel Result { get; set; }
    }

    public class ResultModel
    {
        [JsonProperty(PropertyName = "order")]
        public OrderModel Order { get; set; }

        [JsonProperty(PropertyName = "location")]
        public LocationModel Location { get; set; }
    }

    public class OrderModel
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }

    public class LocationModel
    {
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public List<ParameterItem> Parameters { get; set; }
    }

    public class ParameterItem
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string PValue { get; set; }
    }
}