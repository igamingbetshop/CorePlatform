using IqSoft.CP.AgentWebApi.Filters;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiAgentStatesOutput : ApiResponseBase
    {
        public int UnreadMessagesCount { get; set; }
        public int PaymentRequestsCount { get; set; }
    }
}