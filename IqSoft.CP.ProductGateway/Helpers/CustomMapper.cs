using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using IqSoft.CP.Integration.EcopayzServiceReference;
using IqSoft.CP.ProductGateway.Models.EveryMatrix;
using System;
using System.Linq;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class CustomMapper
    {
        public static AuthorizationOutput MapToAuthorizationOutput(this BllClient client, string token, bool isShopWallet)
        {
            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
            return new AuthorizationOutput
            {
                ClientId = client.Id.ToString(),
                CurrencyId = client.CurrencyId,
                CurrencySymbol = currency.Symbol,
                Token = token,
				UserName = client.UserName,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Gender = client.Gender ?? (int)GenderType.Male,
                BirthDate = client.BirthDate,
                CategoryId = client.CategoryId,
                CategoryName = client.CategoryName,
                UserId = client.UserId,
                IsShopWallet = isShopWallet
            };
        }

        public static ApiBalance ToApiBalance(this BllClientBalance balance)
        {
            return new ApiBalance
            {
                AvailableBalance = balance.AvailableBalance,
                Balances = balance.Balances.Select(x => new ApiAccount
                {
                    Id = x.Id,
                    TypeId = x.TypeId,
                    CurrencyId = x.CurrencyId,
                    Balance = x.Balance,
                    BetShopId = x.BetShopId,
                    PaymentSystemId = x.PaymentSystemId
                }).ToList()
            };
        }

        public static User ToUser(this BllUser input)
        {
            return new User
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Gender = input.Gender,
                CurrencyId = input.CurrencyId,
                Type = input.Type
            };
        }

        public static BonusTicketInfo ToTicketInfo(this BetPayload input, decimal amount)
        {
            return new BonusTicketInfo
            {
                SelectionsCount = Convert.ToInt32(input.subBetCount),
                BetType = (int)CreditDocumentTypes.Single,
                Price = Convert.ToDecimal(input.combination[0].odds),
                BetAmount = amount,
                BetSelections = input.selectionByOutcomeId.Select(x => new BonusTicketSelection
                {
                    SportId = Convert.ToInt32(x.Value.disciplineId),
                    RegionId = Convert.ToInt32(x.Value.locationId),
                    CompetitionId = x.Value.tournamentId,
                    MatchId = x.Value.eventId,
                    SelectionId = x.Value.outcomeId,
                    MarketTypeId = Convert.ToInt32(x.Value.marketId),
                    MatchStatus = x.Value.liveMatch ? 1 : 0,
                    Price = Convert.ToDecimal(x.Value.odds)
                }).ToList() 
            };
        }
    }
}