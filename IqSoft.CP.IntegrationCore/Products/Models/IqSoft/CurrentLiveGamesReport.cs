using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.IqSoft
{
    public class CurrentLiveGamesReport
    {
        public List<Data> RoundResult { get; set; }
    }

    public class Data
    {
        public string RoundId { get; set; }
        public string TableID { get; set; }
        public string TableName { get; set; }
        public string RoundDateTime { get; set; }
        public string DealerId { get; set; }
        public string DealerName { get; set; }
        public Round Results { get; set; }
        public int RoundDuration { get; set; }
    }

    public class Round
    {
        public List<object> DealerCards { get; set; }
        public List<object> PlayerCards { get; set; }
        public List<object> BankerCards { get; set; }
        public List<object> SeatCards { get; set; }
        public List<object> CommunityCards { get; set; }
        public List<object> WinningBalls { get; set; }
        public List<object> Outcomes { get; set; }
        public object Info { get; set; }
    }
}
