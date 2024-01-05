namespace IqSoft.CP.ProductGateway.Models.Groove
{
    public class BaseInput
    {
        public string gamesessionid { get; set; }
        public string accountid { get; set; }
        public string device { get; set; }
        public string apiversion { get; set; }
        public string request { get; set; }
        public decimal? betamount { get; set; }
        public decimal? amount { get; set; }
        public string gameid { get; set; }
        public string roundid { get; set; }
        public string transactionid { get; set; }
        public string frbid { get; set; }
        public string gamestatus { get; set; }
        public decimal? result { get; set; }
    }
}