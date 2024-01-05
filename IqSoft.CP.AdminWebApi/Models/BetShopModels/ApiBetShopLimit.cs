namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class ApiBetShopLimit
    {
        public int UserId { get; set; }
        public int BetShopId { get; set; }
        public decimal CurrentLimit { get; set; }
    }
}