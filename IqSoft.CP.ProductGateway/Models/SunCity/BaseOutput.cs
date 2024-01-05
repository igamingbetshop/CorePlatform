using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.SunCity
{
    public class BaseOutput
    {
        public BaseOutput()
        {
            Users = new List<OutputUser>();
        }

        [JsonProperty(PropertyName = "err")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errdesc")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "users")]
        public List<OutputUser> Users { get; set; }
    }

    public class OutputUser
    {
        public OutputUser()
        {
            Wallets = new List<Wallet>();
        }

        [JsonProperty(PropertyName = "err")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errdesc")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "wallets")]
        public List<Wallet> Wallets { get; set; }
    }

    public class Wallet
    {
        [JsonProperty(PropertyName = "code")]
        public string WalletCode { get; set; }

        [JsonProperty(PropertyName = "bal")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "cur")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}