namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class ApiCreateMessageInput : ApiRequestBase
    {
        public int TicketId { get; set; }

        public string Message { get; set; }
    }
}