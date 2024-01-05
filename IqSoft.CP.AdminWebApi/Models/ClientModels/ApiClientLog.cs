namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientLog
    {
        public long Id { get; set; }
        public int Action { get; set; }
        public int ClientId { get; set; }
        public int? UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string Ip { get; set; }
        public string Page { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long? SessionId { get; set; }
    }
}