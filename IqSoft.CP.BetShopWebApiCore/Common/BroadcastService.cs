using System.Linq;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using Serilog;
using Microsoft.AspNetCore.SignalR;

namespace IqSoft.CP.BetShopWebApi.Common
{
    public class BroadcastService
    {
        public static void BroadcastBet(PlaceBetOutput bet)
        {
            Log.Logger.Information("BroadcastBet_" + bet.CashierId);
            var connectionIds = BaseHub.ConnectedClients.FirstOrDefault(x => x.Value.CashierId == bet.CashierId).Value.ConnectionIds;
            if (connectionIds != null && connectionIds.Any())
            {
                foreach (var cId in connectionIds)
                {
                    BaseHub.CurrentContext.Clients.Client(cId).SendAsync("onBet", bet.Bets[0]);
                }
            }
        }
    }
}