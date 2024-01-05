namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class Api2FAInput : RequestBase
    {
        public string Pin { get; set; }
        public int AgentId { get; set; }
    }
}