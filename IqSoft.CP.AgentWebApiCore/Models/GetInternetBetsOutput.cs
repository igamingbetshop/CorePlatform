using System;
using System.Collections.Generic;
using IqSoft.CP.Common.Attributes;

namespace IqSoft.CP.AgentWebApi.Models
{
    //public class GetInternetBetsOutput
    //{
    //    public List<EnumerationModel<int>> States { get; set; }
    //    public List<InternetBetModel> Bets { get; set; }
    //}

    public class InternetBetModel
    {
        public long BetDocumentId { get; set; }
        public int State { get; set; }

        [NotExcelProperty]
        public string BetInfo { get; set; }
        public int ProductId { get; set; }

        [NotExcelProperty]
        public int? GameProviderId { get; set; }

        [NotExcelProperty]
        public string TicketInfo { get; set; }
        public DateTime BetDate { get; set; }

        [NotExcelProperty]
        public DateTime? CalculationDate { get; set; }
        public int ClientId { get; set; }
        public string ClientUserName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public decimal BetAmount { get; set; }
        public decimal? WinAmount { get; set; }
        public string CurrencyId { get; set; }
        public int? BonusId { get; set; }

        [NotExcelProperty]
        public long? TicketNumber { get; set; }

        [NotExcelProperty]
        public int? DeviceTypeId { get; set; }
        public int? BetTypeId { get; set; }
        public decimal? PossibleWin { get; set; }

        [NotExcelProperty]
        public int PartnerId { get; set; }
        public string ProductName { get; set; }
        public string ProviderName { get; set; }
        public string ClientIp { get; set; }
        public string Country { get; set; }
        public int? ClientCategoryId { get; set; }
        public decimal Profit { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        [NotExcelProperty]
        public bool HasNote { get; set; }

        [NotExcelProperty]
        public string RoundId { get; set; }

        [NotExcelProperty]
        public bool ClientHasNote { get; set; }
    }
}