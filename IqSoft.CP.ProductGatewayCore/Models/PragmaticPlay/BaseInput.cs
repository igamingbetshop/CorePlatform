namespace IqSoft.CP.ProductGateway.Models.PragmaticPlay
{
    public class BaseInput
    {
        public string hash { get; set; }
        public string token { get; set; }
        public string providerId { get; set; }
        public string gameId { get; set; }
        public string ipAddress { get; set; }
        public int? userId { get; set; }
    }
}