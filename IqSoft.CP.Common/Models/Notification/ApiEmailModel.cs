namespace IqSoft.CP.Common.Models
{
    public class ApiEmailModel
    {
        public int ParnerId { get; set; }
        public string ClientId { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ExternalTemplateId { get; set; }
        public string AttachedFileName { get; set; }
        public string AttachedContent { get; set; }
    }
}
