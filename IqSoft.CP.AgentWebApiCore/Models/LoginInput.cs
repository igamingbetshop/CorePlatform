namespace IqSoft.CP.AgentWebApi.Models
{
    public class LoginInput
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ReCaptcha { get; set; }
    }
}