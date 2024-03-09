namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class CashDeskOperations : ApiFilterBase
    {
        public int? LastShiftsNumber { get; set; }

        public int CashierId { get; set; }
    }
}