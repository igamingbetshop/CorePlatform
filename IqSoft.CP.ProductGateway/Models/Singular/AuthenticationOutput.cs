using System.Runtime.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    [DataContract(Name = "authByTokenResponseItem")]
    public class AuthenticationOutput : BaseOutput
    {
        [DataMember(Name = "userID")]
        public long UserId { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "userIP")]
        public string UserIp { get; set; }

        [DataMember(Name = "PreferredCurrencyID")]
        public short PreferredCurrencyId { get; set; }
    }
}