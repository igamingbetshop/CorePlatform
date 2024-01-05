namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class RefreshTokenInput : InputBase
    {
        public string Token { get; set; }
        public int ClientId { get; set; }
    }
}
