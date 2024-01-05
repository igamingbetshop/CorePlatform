using System;

namespace IqSoft.CP.ProductGateway.Models.EkkoSpin
{
    public class AdditionalInfo
    {
        public string SlotBettingType { get; set; }
        public string BettingPoolType { get; set; }
        public string BettingType { get; set; }
        public int SportId { get; set; }
        public string SportTitle { get; set; }
        public int LocationId { get; set; }
        public string LocationTitle { get; set; }
        public string CountryCode { get; set; }
        public int LeagueId { get; set; }
        public string LeagueTitle { get; set; }
        public int RaceNumber { get; set; }
        public DateTime RaceStartDate { get; set; }
        public string FirstPlace { get; set; }
        public string SecondPlace { get; set; }
        public string ThirdPlace { get; set; }
        public string AnyPlace { get; set; }
    }
}