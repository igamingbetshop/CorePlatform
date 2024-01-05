using System;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.Integration.Products.Models
{
    public class BetSelection
    {
        public int RoundId { get; set; }

        public int UnitId { get; set; }

        public string UnitName { get; set; }

        public string RoundName { get; set; }

        public int MarketTypeId { get; set; }

        public long MarketId { get; set; }

        public string MarketName { get; set; }

        public long SelectionTypeId { get; set; }

        public long SelectionId { get; set; }

        public string SelectionName { get; set; }

        public decimal Coefficient { get; set; }

        [JsonConverter(typeof(DateTimeHelper.CustomDateTimeConverter))]
        public DateTime? EventDate { get; set; }
    }
}