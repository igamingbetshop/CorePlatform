namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class ApiGameInfo
    {
        public long Id { get; set; }

        public int GameId { get; set; }

        public string GameName { get; set; }

        public int? UnitId { get; set; }

        public int RoundId { get; set; }

        public int Status { get; set; }

        public System.DateTime CreationDate { get; set; }
    }
}