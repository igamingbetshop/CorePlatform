namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class TicketMessageModel
    {
        public long Id { get; set; }

        public string Message { get; set; }

        public int Type { get; set; }

        public System.DateTime CreationTime { get; set; }

        public long TicketId { get; set; }

        public int? UserId { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }
    }
}