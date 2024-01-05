namespace IqSoft.CP.ProductGateway.Models.EveryMatrix
{
    public class BaseOutput
    {
        public string ApiVersion { get; set; } = "1.0";
        public string Request { get; set; }
        public int ReturnCode { get; set; }       
        public string SessionId { get; set; }
        public string Message { get; set; }
    }
}