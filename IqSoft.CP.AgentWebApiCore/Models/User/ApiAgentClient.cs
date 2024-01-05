namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ApiAgentClient
    {
        public int ClientId { get; set; }
        public string ClientUserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int State { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public System.DateTime RegistrationDate { get; set; }
        public int AgentId { get; set; }
        public decimal Balance { get; set; }
    }
}