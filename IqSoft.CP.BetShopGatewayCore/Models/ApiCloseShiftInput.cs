namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiCloseShiftInput : RequestBase
    {
        public int CashDeskId { get; set; }
        
        public int CashierId { get; set; }
    }
}