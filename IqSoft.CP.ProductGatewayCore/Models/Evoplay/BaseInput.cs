namespace IqSoft.CP.ProductGateway.Models.Evoplay
{
    public class BaseInput
    {
        public string token { get; set; }
        public string callback_id { get; set; }
        public string name { get; set; }
        public string signature { get; set; }
        public TransactionDetails data { get; set; }
    }

    public class TransactionDetails
    {
        public string round_id { get; set; }
        public string action_id { get; set; }
        public string final_action { get; set; }
        public string refund_round_id { get; set; }
        public string refund_action_id { get; set; }
        public string refund_callback_id { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string win_action_id { get; set ; }
        public string win_final_action { get; set ; }
        public string details { get; set; }
    }

}