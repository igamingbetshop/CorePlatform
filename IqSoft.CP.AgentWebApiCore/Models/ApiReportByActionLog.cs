
namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiReportByActionLog
    {
        public long Id { get; set; }
        public string ActionName { get; set; }
        public int ActionGroup { get; set; }
        public long? UserId { get; set; }
        public string Domain { get; set; }
        public string Source { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public long? SessionId { get; set; }
        public string Page { get; set; }
        public string Language { get; set; }
        public int? ResultCode { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public System.DateTime CreationTime { get; set; }
    }
}