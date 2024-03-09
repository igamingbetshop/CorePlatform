using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterBetShopBet : FilterBase<fnBetShopBet>
    {
        public int? PartnerId { get; set; }
        public int? AgentId { get; set; }
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation CashierIds { get; set; }

        public FiltersOperation CashDeskIds { get; set; }

        public FiltersOperation BetShopIds { get; set; }

        public FiltersOperation BetShopNames { get; set; }

        public FiltersOperation BetShopGroupIds { get; set; }

        public FiltersOperation BetShopGroupNames { get; set; }

        public FiltersOperation ProductIds { get; set; }

        public FiltersOperation ProductNames { get; set; }

        public FiltersOperation ProviderIds { get; set; }

        public FiltersOperation ProviderNames { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation RoundIds { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation BetTypes { get; set; }

        public FiltersOperation PossibleWins { get; set; }

        public FiltersOperation BetAmounts { get; set; }

        public FiltersOperation WinAmounts { get; set; }
        public FiltersOperation OriginalBetAmounts { get; set; }

        public FiltersOperation OriginalWinAmounts { get; set; }

        public FiltersOperation Barcodes { get; set; }

        public FiltersOperation TicketNumbers { get; set; }

        public FiltersOperation BetDates { get; set; }

        public override void CreateQuery(ref IQueryable<fnBetShopBet> objects, Func<IQueryable<fnBetShopBet>, IOrderedQueryable<fnBetShopBet>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (AgentId.HasValue)
                objects = objects.Where(x => x.AgentId == AgentId.Value);

            var fDate = FromDate.Year * (long)1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * (long)1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);

			FilterByValue(ref objects, Ids, "BetDocumentId");
            FilterByValue(ref objects, CashierIds, "CashierId");
            FilterByValue(ref objects, CashDeskIds, "CashDeskId");
            FilterByValue(ref objects, BetShopIds, "BetShopId");
            FilterByValue(ref objects, BetShopNames, "BetShopName");
            FilterByValue(ref objects, BetShopGroupIds, "BetShopGroupId");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, ProviderIds, "GameProviderId");
            FilterByValue(ref objects, ProviderNames, "ProviderName");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, RoundIds, "RoundId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, BetTypes, "BetTypeId");
            FilterByValue(ref objects, PossibleWins, "PossibleWin");
            FilterByValue(ref objects, BetAmounts, "BetAmount");
            FilterByValue(ref objects, WinAmounts, "WinAmount");
            FilterByValue(ref objects, OriginalBetAmounts, "OriginalBetAmount");
            FilterByValue(ref objects, OriginalWinAmounts, "OriginalWinAmount");
            if (Barcodes != null && Barcodes.OperationTypeList != null && Barcodes.OperationTypeList.Any())
            {
                foreach (var item in Barcodes.OperationTypeList)
                {
                    item.IntValue = item.IntValue - 1000000000000;
                    item.IntValue /= 10;
                }
            }
            FilterByValue(ref objects, Barcodes, "BetDocumentId");
            FilterByValue(ref objects, TicketNumbers, "TicketNumber");
            if (BetDates != null && BetDates.OperationTypeList != null && BetDates.OperationTypeList.Any())
            {
                foreach (var item in BetDates.OperationTypeList)
                {
                    item.IntValue = item.DateTimeValue.Year * 1000000 + item.DateTimeValue.Month * 10000 + item.DateTimeValue.Day * 100 + item.DateTimeValue.Hour;
                }
            }
            FilterByValue(ref objects, BetDates, "Date");

            base.FilteredObjects(ref objects, orderBy);
        }

        public IQueryable<fnBetShopBet> FilterObjects(IQueryable<fnBetShopBet> betShopBets, Func<IQueryable<fnBetShopBet>, IOrderedQueryable<fnBetShopBet>> orderBy = null)
        {
            CreateQuery(ref betShopBets, orderBy);
            return betShopBets;
        }

        public IQueryable<fnBetShopBet> FilterObjectsTotals(IQueryable<fnBetShopBet> betShopBets)
        {
            CreateQuery(ref betShopBets);
            return betShopBets;
        }
    }
}
