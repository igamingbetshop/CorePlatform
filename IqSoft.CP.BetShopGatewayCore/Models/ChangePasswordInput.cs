namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ChangePasswordInput : RequestBase
    {
        public int CashDeskId { get; set; }
        public int CashierId { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
}