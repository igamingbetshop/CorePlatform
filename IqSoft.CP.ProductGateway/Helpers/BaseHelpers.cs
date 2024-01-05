using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class BaseHelpers
    {
        public static readonly dynamic _connectedClients =
        GlobalHost.ConnectionManager.GetHubContext<BaseHub>().Clients.Group("WebSiteWebApi");

        public static decimal GetClientProductBalance(int clientId, int productId)
        {
            bool allowBonus = false;

            if (productId > 0)
            {
                var cacheBonus = CacheManager.GetActiveWageringBonus(clientId);
                if (cacheBonus.Id > 0)
                {
                    var clientBonus = CacheManager.GetBonusById(cacheBonus.BonusId);
                    if (clientBonus.FreezeBonusBalance != true)
                    {
                        if (clientBonus.Type == (int)BonusTypes.CampaignWagerSport && (productId == Constants.SportsbookProductId || Constants.ExternalSportsbookProductIds.Contains(productId)))
                            allowBonus = true;
                        else if (clientBonus.Type != (int)BonusTypes.CampaignWagerSport && productId != Constants.SportsbookProductId && !Constants.ExternalSportsbookProductIds.Contains(productId))
                        {
                            var bonusProducts = CacheManager.GetBonusProducts(cacheBonus.BonusId);
                            var product = CacheManager.GetProductById(productId);
                            while (!allowBonus)
                            {
                                var pr = bonusProducts.FirstOrDefault(x => x.ProductId == product.Id);
                                if (pr != null)
                                {
                                    if (pr.Percent > 0)
                                        allowBonus = true;
                                    break;
                                }
                                else
                                {
                                    if (!product.ParentId.HasValue)
                                        break;
                                    product = CacheManager.GetProductById(product.ParentId.Value);
                                }
                            }
                        }
                    }
                }
            }
            if (!allowBonus)
                return Math.Floor(CacheManager.GetClientCurrentBalance(clientId).Balances.Where(x =>
                    x.TypeId != (int)AccountTypes.ClientCompBalance &&
                    x.TypeId != (int)AccountTypes.ClientCoinBalance &&
                    x.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(x => x.Balance) * 100) / 100;
            else
                return CacheManager.GetClientCurrentBalance(clientId).AvailableBalance;
        }

        public static void BroadcastWin(ApiWin input)
        {
            var currency = CacheManager.GetCurrencyById(input.CurrencyId);
            input.CurrencyId = currency.Name;
            if (input.Amount > 0)
            {
                input.ApiBalance = CacheManager.GetClientCurrentBalance(input.ClientId).ToApiBalance();
                _connectedClients.BroadcastWin(input);
            }
        }

        public static void BroadcastBalance(int clientId, decimal? balance = null)
        {
            if (balance != null)
                _connectedClients.BroadcastBalance(new ApiWin { ClientId = clientId, 
                    ApiBalance = new ApiBalance 
                    { 
                        AvailableBalance = balance.Value, 
                        Balances = new System.Collections.Generic.List<ApiAccount> { new ApiAccount { TypeId = (int)AccountTypes.ClientUsedBalance, Balance = balance.Value } } 
                    } });
            else
            {
                var clientBalance = CacheManager.GetClientCurrentBalance(clientId);
                _connectedClients.BroadcastBalance(new ApiWin { ClientId = clientId, ApiBalance = clientBalance.ToApiBalance() });
            }
        }

        public static void BroadcastBetLimit(LimitInfo info)
        {
            if (info.DailyBetLimitPercent != null || info.WeeklyBetLimitPercent != null || info.MonthlyBetLimitPercent != null)
            {
                _connectedClients.BroadcastBetLimit(info);
            }
        }

        public static void RemoveFromeCache(string key)
        {
            InvokeMessage("RemoveKeyFromCache", key);
        }

        public static void RemoveSessionFromeCache(string token, int? productId)
        {
            InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_0", Constants.CacheItems.ClientSessions, token));
            
            if(productId != null)  
                InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, token, productId));
        }

        public static void RemoveClientBalanceFromeCache(int clientId)
        {
            InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId));
        }

        private static void InvokeMessage(string messageName, params object[] obj)
        {
            Task.Run(() => WebApiApplication.JobHubProxy.Invoke(messageName, obj));
        }
    }
}