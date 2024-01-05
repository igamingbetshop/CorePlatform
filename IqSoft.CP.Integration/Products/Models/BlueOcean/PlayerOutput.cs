namespace IqSoft.CP.Integration.Products.Models.BlueOcean
{
    public class PlayerOutput : BaseOutput
    {
        public Player response { get; set; }
    }

    public class Player
    {
        public string id { get; set; }
    }
}