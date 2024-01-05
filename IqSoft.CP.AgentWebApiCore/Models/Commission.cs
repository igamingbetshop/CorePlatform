using IqSoft.CP.Common.Models.AgentModels;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class LevelLimit
    {
        public int Level { get; set; }
        public decimal? Limit { get; set; }
    }
    public class CountLimit
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }
}