namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiTicket
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public string Subject { get; set; }
        public int Type { get; set; }
        public System.DateTime CreationTime { get; set; }
        public int PartnerId { get; set; }
        public int? ClientId { get; set; }
    }
}