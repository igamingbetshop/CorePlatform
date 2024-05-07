using System.Linq;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.BetShopWebApi.Common
{
    public class BroadcastService
    {
        public static void BroadcastBet(PlaceBetOutput bet)
        {
            var connection = BaseHub.ConnectedClients.FirstOrDefault(x => x.Value.CashierId == bet.CashierId);
            var connectionIds = connection.Value.ConnectionIds;
            if (connectionIds != null && connectionIds.Any())
            {
                foreach (var cId in connectionIds)
                {
                    bet.Bets[0].BetDate = bet.Bets[0].BetDate.GetGMTDateFromUTC(connection.Value.TimeZone);
                    BaseHub.CurrentContext.Clients.Client(cId).SendAsync("onBet", bet.Bets[0]);
                }
            }
        }
    }
}