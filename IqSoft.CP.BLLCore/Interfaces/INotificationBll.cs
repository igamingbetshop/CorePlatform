using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface INotificationBll : IBaseBll
    {
        int RegisterActiveEmail(int partnerId, string email, string subject, string body, int? templateId);
        void UpdateEmailStatus(int emailId, int status);
        bool SendEmail(string clientId, int partnerId, string email, string subject, string body,
            string externalTemplateId, string fileName = "", string fileContent = "");
        bool SendSms(int partnerId, string mobileNumber, string messegeText, long messageId);
        ClientMessage SaveEmailMessage(int partnerId, int clientId, string receiver, string subject, string body, int? templateId);
        void SendInternalTicket(int clientId, int? notificationType, string messageText = "", PaymentNotificationInfo paymentInfo = null);
        int SendVerificationCodeToEmail(int clientId, string email);
        void SendInvitationToAffiliateClient(int clientId, string email);
        int SendNotificationMessage(NotificationModel notificationModel);
        void SendDepositNotification(int clientId, int requestState, decimal amount, string reason);
        void SendWitdrawNotification(int clientId, int requestState, decimal amount, string reason);
    }
}