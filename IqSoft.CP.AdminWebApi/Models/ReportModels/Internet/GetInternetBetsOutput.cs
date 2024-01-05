using System;
using System.Collections.Generic;
using IqSoft.CP.Common.Attributes;
using IqSoft.CP.AdminWebApi.Models.CommonModels;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.Internet
{
    public class GetInternetBetsOutput
    {
        public List<EnumerationModel<int>> States { get; set; }
        public List<InternetBetModel> Bets { get; set; }
    }

    public class InternetBetModel
    {
        public long BetDocumentId { get; set; }
        public int State { get; set; }

        [NotExcelProperty]
        public string BetInfo { get; set; }
        public int ProductId { get; set; }

        [NotExcelProperty]
        public int? GameProviderId { get; set; }
        public int? SubproviderId { get; set; }
        public string SubproviderName { get; set; }

        [NotExcelProperty]
        public string TicketInfo { get; set; }
        public DateTime BetDate { get; set; }

        [NotExcelProperty]
        public DateTime? CalculationDate { get; set; }
        public int ClientId { get; set; }
        public string ClientUserName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public int? UserId { get; set; }
        public decimal BetAmount { get; set; }
        public decimal OriginalBetAmount { get; set; }
        public decimal Coefficient { get; set; }
        public decimal? WinAmount { get; set; }
        public decimal? OriginalWinAmount { get; set; }
        public decimal? BonusAmount { get; set; }
        public decimal? OriginalBonusAmount { get; set; }
        public int? BonusId { get; set; }
        public string CurrencyId { get; set; }

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
        public decimal? Rake { get; set; }
        public DateTime? LastUpdateTime { get; set; }

        [NotExcelProperty]
        public bool HasNote { get; set; } = true;

        [NotExcelProperty]
        public string RoundId { get; set; }

        [NotExcelProperty]
        public bool ClientHasNote { get; set; }
    }
}