using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Hubs;
using Microsoft.AspNet.SignalR;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class BaseHelpers
    {
        public static readonly dynamic _connectedClients =
        GlobalHost.ConnectionManager.GetHubContext<BaseHub>().Clients.Group("WebSiteWebApi");

        public static void BroadcastBalance(int clientId)
        {
            var balance = CacheManager.GetClientCurrentBalance(clientId);
            _connectedClients.BroadcastBalance(new ApiWin { ClientId = clientId, ApiBalance = balance.ToApiBalance() });
        }

        public static void BroadcastDepositLimit(LimitInfo info)
        {
            if (info.DailyDepositLimitPercent != null || info.WeeklyDepositLimitPercent != null || info.MonthlyDepositLimitPercent != null)
            {
                _connectedClients.BroadcastDepositLimit(info);
            }
        }
    }
}