using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.BetSolutions
{
    public class Game
    {
        public int StatusCode { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public List<GameItem> Games { get; set; }
    }

    public class GameItem
    {
        public string GameId { get; set; }
        public int ProductId { get; set; }
        public bool HasFreeplay { get; set; }
        public string Name { get; set; }
        public string LaunchUrl { get; set; }     
        public bool HasMobileDeviceSupport { get; set; }
        public Thumbnail[] Thumbnails { get; set; }
    }

    public class Thumbnail
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Lang { get; set; }
    }
}
