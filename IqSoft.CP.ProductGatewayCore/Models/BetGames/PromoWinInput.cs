using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.BetGames
{
    public class PromoWinInput : BaseInput
    {
        [XmlElement("params")]
        public PromoInputParams Parameters { get; set; }
    }

    public class PromoInputParams
    {
        [XmlElement("player_id")]
        public int? PlayerId { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }

        [XmlElement("amount")]
        public int Amount { get; set; }

        [XmlElement("promo_transaction_id")]
        public string PromoTransactionId { get; set; }

        [XmlElement("bet_id")]
        public string BetId { get; set; }

        [XmlElement("game_id")]
        public string GameId { get; set; }

        [XmlElement("promo_type")]
        public string PromoType { get; set; }
    }
}