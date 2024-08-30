namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiOpenTicketInput : ApiRequestBase
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
    }
}