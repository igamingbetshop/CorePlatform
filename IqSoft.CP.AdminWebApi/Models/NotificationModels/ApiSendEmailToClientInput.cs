namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
    public class ApiSendEmailToClientInput
    {
        public int ClientId { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }
    }
}