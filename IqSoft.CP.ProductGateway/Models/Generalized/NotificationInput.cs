namespace IqSoft.CP.ProductGateway.Models.WinSystems
{
    public class NotificationInput
    {
        public string ClientId { get; set; }
        public string Message { get; set; }
        public int? Type { get; set; }
        public string Signature { get; set; }
    }
}