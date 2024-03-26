using System.Linq;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using Microsoft.AspNet.SignalR;

namespace IqSoft.CP.BetShopWebApi.Common
{
    public class BroadcastService
    {
        public static IHubContext _context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
        public static void BroadcastBet(PlaceBetOutput bet)
        {
            WebApiApplication.LogWriter.Info("BroadcastBet_" + bet.CashierId);
            var connectionIds = WebApiApplication.Clients.FirstOrDefault(x => x.Value.CashierId == bet.CashierId).Value.ConnectionIds;
            if (connectionIds != null && connectionIds.Any())
            {
                foreach (var cId in connectionIds)
                {
                    _context.Clients.Client(cId).onBet(bet.Bets[0]);
                }
            }
        }
    }
}