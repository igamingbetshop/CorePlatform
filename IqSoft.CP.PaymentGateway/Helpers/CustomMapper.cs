using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using System.Linq;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class CustomMapper
    {
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
    }
}