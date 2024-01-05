using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class DataModel
    {
        [JsonProperty(PropertyName = "data")]
        public List<UserModel> Data { get; set; }
    }
    public class UserModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}
