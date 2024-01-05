using System.Runtime.Serialization;

namespace IqSoft.CP.ProductGateway.Models.TwoWinPower
{
    public class BaseInput
    {
        [DataMember(Name = "action")]
        public string action { get; set; }

        [DataMember(Name = "player_id")]
        public string player_id { get; set; }

        [DataMember(Name = "currency")]
        public string currency { get; set; }

        [DataMember(Name = "game_uuid")]
        public string game_uuid { get; set; }

        [DataMember(Name = "amount")]
        public string amount { get; set; }

        [DataMember(Name = "transaction_id")]
        public string transaction_id { get; set; }

        [DataMember(Name = "bet_transaction_id")]
        public string bet_transaction_id { get; set; }

        [DataMember(Name = "session_id")]
        public string session_id { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "quantity")]
        public int? quantity { get; set; }

        [DataMember(Name = "freespin_id")]
        public string freespin_id { get; set; }
    }
}