namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class TicketModel
    {
        public long Id { get; set; }

        public int Status { get; set; }

        public string Subject { get; set; }

        public int Type { get; set; }

        public System.DateTime CreationTime { get; set; }

        public int PartnerId { get; set; }

        public int? ClientId { get; set; }

        public string UserName { get; set; }

        public System.DateTime LastMessageTime { get; set; }
        
        public int UnreadMessagesCount { get; set; }

        public string StatusName { get; set; }
        public string TypeName { get; set; }
        public int? UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
    }
}