using IqSoft.CP.AdminWebApi.Models.CommonModels;

namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
    public class ApiReplayMessageInput : ApiRequestBase
    {
        public int MessageId { get; set; }

        public string Text { get; set; }
    }
}