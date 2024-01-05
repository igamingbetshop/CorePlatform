namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class ChangePasswordInput : PlatformRequestBase
    {
        public int CashDeskId { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
}