namespace IqSoft.CP.ProductGateway.Models.Mancala
{
    public class BaseOutput
    {
        public int Error { get; set; } = 0;
        public decimal Balance { get; set; }
        public string Msg { get; set; }
    }
}