namespace IqSoft.CP.Integration.Products.Models.BetDeal
{
    public class SessionInput : BaseInput
    {
        public int userId { get; set; }

        public string nick { get; set; }

        public string authType { get; set; }

        public string lang { get; set; }

       // public string launchOptions { get; set; }
    }
}
