namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class AuthorizationInput : InputBase
    {
        public string ClientIdentifier { get; set; }

        public string Password { get; set; }
        
        public string Ip { get; set; }
        
        public string Token { get; set; }
    }
}
