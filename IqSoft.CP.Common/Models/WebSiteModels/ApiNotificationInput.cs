namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiNotificationInput : ApiRequestBase
    {
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string NotificationServiceType { get; set; }
        public string Code { get; set; }
        public int? ClientId { get; set; }
        public string Token { get; set; }
        public int Type { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
    }
}
