namespace IqSoft.CP.ProductGateway.Models.BlueOcean
{
    public class BaseOutput
    {
        public int status { get; set; }
        public decimal balance { get; set; }
        public string msg { get; set; }
        public string transaction_id { get; set; }
    }
}