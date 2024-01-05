using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.Betsy
{
    public class BetInput : BalanceInput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal? Amount { get; set; }

        [JsonProperty(PropertyName = "info")]
        public string BetInfo { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public List<metadata> Metadatas { get; set; }

        [JsonProperty(PropertyName = "coefficient")]
        public string Coefficient { get; set; }

        [JsonProperty(PropertyName = "ip")]
        public string UserIp { get; set; }

        [JsonProperty(PropertyName = "resultType")]
        public string ResultType { get; set; }

        [JsonProperty(PropertyName = "bonusId")]
        public int? BonusId { get; set; }

        [JsonProperty(PropertyName = "bonusTemplateId")]
        public int? BonusTemplateId { get; set; }

        [JsonProperty(PropertyName = "gameType")]
        public string GameType { get; set; }


        [JsonProperty(PropertyName = "error")]
        public int? ErrorCode { get; set; }
    }

    //public class metadata
    //{
    //    public string disciplineId { get; set; }
    //    public string discipline { get; set; }
    //    public string tournamentId { get; set; }
    //    public string tournament { get; set; }
    //    public string tournamentRegionCode { get; set; }
    //    public string eventId { get; set; }
    //    public string @event { get; set; }
    //    public string eventDate { get; set; }
    //    public string marketId { get; set; }
    //    public string market { get; set; }
    //    public int? outcomeId { get; set; }
    //    public string outcome { get; set; }
    //    public decimal? coefficient { get; set; }
    //    public int stage { get; set; }
    //}

    public class metadata
    {
        public string disciplineId { get; set; }
        public string discipline { get; set; }
        public string tournamentId { get; set; }
        public string tournament { get; set; }
        public string eventId { get; set; }
        public string @event { get; set; }
        public string marketId { get; set; }
        public string eventDate { get; set; }
        public string tournamentRegionCode { get; set; }
        public string market { get; set; }
        public int? outcomeId { get; set; }
        public string outcome { get; set; }
        public decimal? coefficient { get; set; }
        public int stage { get; set; }      
    }
}