namespace IqSoft.CP.ProductGateway.Models.SoftGaming
{
    public class BaseInput
    {
        public string type { get; set; }
        public string userid { get; set; }
        public string currency { get; set; }
        public string i_extparam { get; set; }
        public string i_gamedesc { get; set; }
        public string tid { get; set; }
        public string amount { get; set; }
        public string i_gameid { get; set; } //RoundId
        public string i_actionid { get; set; }
        public string game_extr { get; set; }
        public string game_extra { get; set; }
        public string subtype { get; set; }
        public string i_rollback { get; set; }
        public string jackpot_win { get; set; }
        public string hmac { get; set; }
        public string error { get; set; }
    }
}