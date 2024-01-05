namespace IqSoft.CP.AdminWebApi.Models.AgentModels
{
    public class ChangeObjectStateInput
    {
        public int ObjectId { get; set; }

        public int State { get; set; }

        public string Passsword { get; set; }
    }
}