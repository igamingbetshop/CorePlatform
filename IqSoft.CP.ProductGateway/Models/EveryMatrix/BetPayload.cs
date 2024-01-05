using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.EveryMatrix
{
    public class BetPayload
    {
        public string operationId { get; set; }
        public string betId { get; set; }
        public string terminaltype { get; set; }
        public string bonusWalletID { get; set; }
        public List<CombinationItem> combination { get; set; }
        public Dictionary<string, BetPayloadItem> selectionByOutcomeId { get; set; }
        public string minWin { get; set; }
        public string maxWin { get; set; }
        public string totalBetStakeTax { get; set; }
        public string subBetCount { get; set; }
        public string subBetStake { get; set; }
        public string userIp { get; set; }
        public string gicTransId { get; set; }
        public string gameSessionID { get; set; }
        public string status { get; set; }
    }

    public class CombinationItem
    {
        public string id { get; set; }
        public string odds { get; set; }
        public List<string> outcomeIds { get; set; }
    }

    public class BetPayloadItem
    {
        public string outcomeId { get; set; }
        public string eventId { get; set; }
        public string odds { get; set; }
        public string betBuilderOdds { get; set; }
        public string marketId { get; set; }
        public string disciplineId { get; set; }
        public string locationId { get; set; }
        public string tournamentId { get; set; }
        public bool liveMatch { get; set; }
        public string eventPartId { get; set; }
        public List<string> templateIds { get; set; }
    }
}
