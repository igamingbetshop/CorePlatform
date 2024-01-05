using IqSoft.CP.AdminWebApi.Models.CommonModels;

namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
    public class ApiSendMessageInput : ApiRequestBase
    {
        public int PartnerId { get; set; }

        public int? ClientId { get; set; }
        
        public string Text { get; set; }
        
        public string Subject { get; set; }
    }
}