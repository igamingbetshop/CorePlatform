using IqSoft.CP.AdminWebApi.Filters;

namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
    public class ApiSendMessageToClientsInput : ApiFilterfnClient
    {
        public string Message { get; set; }
        public string Subject { get; set; }
    }
}