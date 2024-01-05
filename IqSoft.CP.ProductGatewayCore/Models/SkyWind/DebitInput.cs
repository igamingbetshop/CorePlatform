namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class DebitInput : BaseInput
    {
        public decimal amount { get; set; }

        public string currency_code { get; set; }

        public string trx_id { get; set; }

        public string game_id { get; set; }

        public string event_type { get; set; }

        public long event_id { get; set; }

        public long timestamp { get; set; }

        public string game_type { get; set; }

        public string platform { get; set; }
    }
}