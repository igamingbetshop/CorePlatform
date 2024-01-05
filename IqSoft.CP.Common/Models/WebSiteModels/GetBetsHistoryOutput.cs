using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetBetsHistoryOutput
    {
        public long Count { get; set; }

        public List<BetModel> Bets { get; set; }
    }
}