using IqSoft.CP.AdminWebApi.Models.CommonModels;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiCreateMessageInput : ApiRequestBase
    {
        public int TicketId { get; set; }

        public string Message { get; set; }
    }
}