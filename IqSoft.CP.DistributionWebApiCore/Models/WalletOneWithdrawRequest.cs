namespace IqSoft.CP.DistributionWebApi.Models
{
    public class WalletOneWithdrawRequest : RequestInput
    {
        public string Key { get; set; }

        public string RequestMethod { get; set; }

        public string Url { get; set; }
    }
}