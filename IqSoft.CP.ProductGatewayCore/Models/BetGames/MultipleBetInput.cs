using System.Collections.Generic;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.BetGames
{
    public class MultipleBetInput : BaseInput
    {
        [XmlElement("params")]
        public MultipleBetInputParams Parameters { get; set; }
    }

    public class MultipleBetInputParams
    {
        [XmlElement("amount")]
        public int? Amount { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }

        [XmlElement("subscription_id")]
        public string SubscriptionId { get; set; }

        [XmlElement("subscription_time")]
        public string SubscriptionTime { get; set; }
        [XmlElement("odd")]
        public Odd OddItem { get; set; }

        [XmlElement("is_mobile")]
        public int? IsMobile { get; set; }    

        [XmlElement("game")]
        public Game GameItem { get; set; }

        [XmlElementAttribute("bet")]
        public List<Bet> BetItem { get; set; }
    }
}