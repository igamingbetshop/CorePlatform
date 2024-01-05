namespace IqSoft.CP.ProductGateway.Models.BlueOcean
{
    public class BaseInput
    {
        public string callerId { get; set; }
        public string callerPassword { get; set; }
        public string callerPrefix { get; set; }
        public string action { get; set; }
        public int remote_id { get; set; }
        public int username { get; set; }
        public string session_id { get; set; }
        public string currency { get; set; }
        public decimal? amount { get; set; }
        public string provider { get; set; }
        public string game_id { get; set; }
        public string game_id_hash { get; set; }
        public string transaction_id { get; set; }
        public string round_id { get; set; }
        public int? gameplay_final { get; set; }
        public int? is_freeround_bet { get; set; }
        public int? is_freeround_win { get; set; }
        public int? is_jackpot_win { get; set; }
        public bool? jackpot_win_in_amount { get; set; }
        public bool? jackpot_contribution_in_amount { get; set; }
        public decimal? fee { get; set; }
        public string freeround_id { get; set; }
        public string gamesession_id { get; set; }
        public string original_session_id { get; set; }

        public string key { get; set; }

    }
}