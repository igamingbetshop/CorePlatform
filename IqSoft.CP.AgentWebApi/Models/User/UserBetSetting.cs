using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class UserBetSetting
    {
        public int? ProductId { get; set; }
        public int MinBet { get; set; }
        public int MaxBet { get; set; }
        public int MaxPerMatch { get; set; }
    }

    public class UserParlayBetSetting
    {
        public int? ParlayMinBet { get; set; }
        public int? ParlayMaxBet { get; set; }
        public int ParlayMaxPerMatch { get; set; }
    }

    public class ProductSetting
    {
        public int ProviderId { get; set; }
        public List<UserBetSetting> BetSettings { get; set; }
        public UserParlayBetSetting ParlayBetSetting { get; set; }
    }
}