namespace IqSoft.CP.Integration.Products.Models.Habanero
{
    public class FreeRoundOutput
    {
        public string CoupondId { get; set; }
        public bool Created { get; set; }
        public string Message { get; set; }
        public string CouponCodeCreated { get; set; }
        public Player[] Players { get; set; }
    }

    public class Player
    {
        public string Username { get; set; }
        public bool Redeemed { get; set; }
        public bool Queued { get; set; }
    }

}
