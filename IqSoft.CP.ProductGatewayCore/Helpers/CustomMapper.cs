using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using System.Linq;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class CustomMapper
    {
        public static AuthorizationOutput MapToAuthorizationOutput(this BllClient client, string token)
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
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                CategoryId = client.CategoryId,
                CategoryName = client.CategoryName,
                UserId = client.UserId
            };
        }

        public static ApiBalance ToApiBalance(this BllClientBalance balance)
        {
            return new ApiBalance
            {
                AvailableBalance = balance.AvailableBalance,
                Balances = balance.Balances.Select(x => new ApiAccount
                {
                    TypeId = x.TypeId,
                    CurrencyId = x.CurrencyId,
                    Balance = x.Balance
                }).ToList()
            };
        }
    }
}