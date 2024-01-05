namespace IqSoft.CP.Integration.Products.Models.TwoWinPower
{
    public class FreespinInput
    {
        public string player_id { get; set; }

        public string player_name { get; set; }

        public string currency { get; set; }

        public int quantity { get; set; }

        public string valid_from { get; set; }

        public string valid_until { get; set; }

        public string freespin_id { get; set; }

        public string bet_id { get; set; }

        public decimal denomination { get; set; }

        public string game_uuid { get; set; }
    }
}
