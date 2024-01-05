using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Mancala
{
    public class GamesOutput
    {
        public List<GameItem> Games { get; set; }
        public int Error { get; set; }
        public string Msg { get; set; }
    }
    public class GameItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<string> Images { get; set; }
    }
}