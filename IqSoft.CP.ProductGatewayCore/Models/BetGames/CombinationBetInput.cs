using System.Collections.Generic;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.BetGames
{
    public class CombinationBetInput : BaseInput
    {
        [XmlElement("params")]
        public CombinationBetInputParams Parameters { get; set; }
    }

    public class CombinationBetInputParams
    {
        [XmlElement("player_id")]
        public string PlayerId { get; set; }

        [XmlElement("amount")]
        public int? Amount { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("odd_value")]
        public decimal? OddValue { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }

        [XmlElement("subscription_id")]
        public string SubscriptionId { get; set; }

        [XmlElement("subscription_time")]
        public string SubscriptionTime { get; set; }

        [XmlElement("combination_id")]
        public string CombinationId { get; set; }

        [XmlElement("combination_time")]
        public string CombinationTime { get; set; }

        [XmlElement("is_mobile")]
        public int? IsMobile { get; set; }

        [XmlElement("odd")]
        public Odd OddItem { get; set; }

        [XmlElement("game")]
        public Game GameItem { get; set; }

        [XmlElementAttribute("bet")]
        public List<Bet> BetItem { get; set; }

    }
    public class Bet
    {
        [XmlElement("bet_id")]
        public string BetId { get; set; }

        [XmlElement("game_id")]
        public string GameId { get; set; }

        [XmlElement("transaction_id")]
        public string TransactionId { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("amount")]
        public int? Amount { get; set; }

        [XmlElement("draw")]
        public Draw DrawItem { get; set; }

        [XmlElement("game")]
        public Game GameItem { get; set; }

        [XmlElement("odd")]
        public Odd OddItem { get; set; }

        [XmlElement("bet_option")]
        public List<BetOption> BetOptions { get; set; }
    }

    public class Draw
    {
        [XmlElement("code")]
        public string Code { get; set; }

        [XmlElement("time")]
        public string Time { get; set; }
    }

    public class Odd
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("value")]
        public decimal? Value { get; set; }

        [XmlElement("translation")]
        public string Translation { get; set; }
    }

    public class Game
    {
        [XmlElement("id")]
        public int? Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("translation")]
        public string Translation { get; set; }
    }
}