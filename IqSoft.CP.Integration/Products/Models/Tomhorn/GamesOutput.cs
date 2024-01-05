using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Tomhorn
{
    public class GamesOutput
    {
        public string BaseURL { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public List<GameModule> GameModules { get; set; }
    }

    public class GameModule
    {
        public string Channel { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Provider { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
    }

}