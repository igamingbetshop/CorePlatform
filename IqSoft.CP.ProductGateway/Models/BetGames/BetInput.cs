using System.Collections.Generic;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.BetGames
{
    public class BetInput : BaseInput
    {
        [XmlElement("params")]
        public BetInputParams Parameters { get; set; }
    }

    public class BetInputParams
    {
        [XmlElement("player_id")]
        public int? PlayerId { get; set; }

        [XmlElement("amount")]
        public int Amount { get; set; }

        [XmlElement("type")]
        public string type { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }
        
        [XmlElement("bet_id")]
        public string BetId { get; set; }

        [XmlElement("transaction_id")]
        public string TransactionId { get; set; }

        [XmlElement("retrying")]
        public int Retrying { get; set; }

        [XmlElement("bet_type")]
        public string BetType { get; set; }

        [XmlElement("game_id")]
        public string GameId { get; set; }

        [XmlElement("bet")]
        public string Bet { get; set; }

        [XmlElement("odd")]
        public decimal? Odd { get; set; }

        [XmlElement("bet_time")]
        public string BetTime { get; set; }

        [XmlElement("game")]
        public string Game { get; set; }

        [XmlElement("draw_code")]
        public string DrawCode { get; set; }

        [XmlElement("draw_time")]
        public string DrawTime { get; set; }     

        [XmlElement("bet_option")]
        public List<BetOption> BetOptions { get; set; }
    }

    public class BetOption
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("round")]
        public string Round { get; set; }
    }
}