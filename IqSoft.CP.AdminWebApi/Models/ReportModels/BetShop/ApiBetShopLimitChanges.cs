namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiBetShopLimitChanges
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public long BetShopId { get; set; }
        public decimal? LimitValue { get; set; }
        public System.DateTime CreationTime { get; set; }
    }
}