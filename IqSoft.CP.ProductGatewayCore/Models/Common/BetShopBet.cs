using System;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.ProductGateway.Models.Common
{
    public class BetShopBet
    {
        public string Token { get; set; }

        public List<BetOutput> Bets { get; set; }

        public decimal Balance { get; set; }

        public decimal CurrentLimit { get; set; }
    }

    public class BetOutput : ResponseBase
    {
        public string TransactionId { get; set; }

        public long Barcode { get; set; }

        public long TicketNumber { get; set; }

        public int GameId { get; set; }

        public string GameName { get; set; }

        public decimal Amount { get; set; }

        [JsonConverter(typeof (DateTimeHelper.CustomDateTimeConverter))]
        public DateTime BetDate { get; set; }

        public decimal Balance { get; set; }

        public decimal CurrentLimit { get; set; }

        public decimal Coefficient { get; set; }

        public int BetType { get; set; }

        public string Info { get; set; }

        public List<BllBetSelection> BetSelections { get; set; }
    }

    public class BllBetSelection
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

        [JsonConverter(typeof (DateTimeHelper.CustomDateTimeConverter))]
        public DateTime? EventDate { get; set; }
    }
}