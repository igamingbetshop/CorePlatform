namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ChangePasswordInput
    {
        public int? UserIdentity { get; set; }
        public int? ClientIdentity { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
}