using System;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.BetGames
{
    [Serializable]
    [XmlRootAttribute("root", Namespace = "", IsNullable = false)]
    public class BaseOutput
    {
        [XmlElement("method")]
        public string Method { get; set; }

        [XmlElement("token")]
        public string Token { get; set; }

        [XmlElement("success")]
        public int Success { get; set; }

        [XmlElement("error_code")]
        public int ErrorCode { get; set; }

        [XmlElement("error_text")]
        public string ErrorText { get; set; }

        [XmlElement("time")]
        public int Time { get; set; }

        [XmlElement("signature")]
        public string Signature { get; set; }

        [XmlElement("params")]
        public AccountDetailsOutputParams Parameters { get; set; }
    }

    public class AccountDetailsOutputParams
    {
        [XmlElement("user_id")]
        public string UserId { get; set; }

        [XmlElement("username")]
        public string UserName { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }

        [XmlElement("info")]
        public string Info { get; set; }
        [XmlElement("new_token")]
        public string NewToken { get; set; }

        [XmlElement("balance")]
        public string Balance { get; set; }

        [XmlElement("balance_after")]
        public string BalanceAfter { get; set; }

        [XmlElement("already_processed")]
        public string AlreadyProcessed { get; set; }
    }
}