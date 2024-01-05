namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ChangeObjectStateInput
    {
        public int ObjectId { get; set; }
        public int? ObjectTypeId { get; set; }
        public int State { get; set; }
        public string Password { get; set; }
    }
}