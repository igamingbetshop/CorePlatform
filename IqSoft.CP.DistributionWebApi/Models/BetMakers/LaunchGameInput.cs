namespace IqSoft.CP.DistributionWebApi.Models.BetMakers
{
    public class LaunchGameInput
    {
        public bool LoggedIn { get; set; }
        public string AccessToken { get; set;}
        public decimal Balance { get; set;}
        public decimal BonusBalance { get; set;}
        public bool ShowBalance { get; set;}
        public string BrandName { get; set;}
        public string BrandUserId { get; set;}
        public string UserId { get; set;}

    }
}