using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.DAL.Models.Notification
{
    public class NotificationModel
    {
        public int PartnerId { get; set; }
        public int? ClientId { get; set; }
        public int? AffiliateId { get; set; }
        public string MobileOrEmail { get; set; }
        public int? ClientInfoType { get; set; }
        public string VerificationCode { get; set; }
        public string Parameters { get; set; }
        public string MessageText { get; set; }
        public int? SubjectType { get; set; }
        public string LanguageId { get; set; }
        public int MessageType { get; set; } = (int)ClientMessageTypes.Sms;
        public PaymentNotificationInfo PaymentInfo { get; set; }
    }
}
