namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class TicketMessageModel
    {
        public long Id { get; set; }

        public string Message { get; set; }

        public int Type { get; set; }

        public System.DateTime CreationTime { get; set; }

        public long TicketId { get; set; }

		public bool IsFromUser { get; set; }

	}
}