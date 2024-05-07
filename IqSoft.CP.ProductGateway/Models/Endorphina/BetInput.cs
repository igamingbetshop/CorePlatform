namespace IqSoft.CP.ProductGateway.Models.Endorphina
{
    public class BetInput : BaseInput
    {
        public long amount { get; set; }

        public string date { get; set; }
        
        public int gameId { get; set; }

        public long id { get; set; }
    }
}