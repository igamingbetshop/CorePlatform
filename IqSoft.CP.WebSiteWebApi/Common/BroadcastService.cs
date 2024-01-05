﻿using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.CommonCore.Models.WebSiteModels;
using IqSoft.CP.WebSiteWebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IqSoft.CP.WebSiteWebApi.Common
{
	public class BroadcastService
    {
        public static void BroadcastWin(ApiWin win, object message)
		{
			Thread.Sleep(5000);
			BaseHub.CurrentContext.Clients.Group("Partner_" + win.PartnerId).SendAsync("onWin", message);
			var balance = win.ApiBalance;
			balance.AvailableBalance = Math.Floor(balance.Balances.Where(x => x.TypeId != (int)AccountTypes.ClientBonusBalance &&
																x.TypeId != (int)AccountTypes.ClientCompBalance &&
																x.TypeId != (int)AccountTypes.ClientCoinBalance)
													.Sum(x => x.Balance) * 100) / 100;
			BaseHub.CurrentContext.Clients.Group("Client_" + win.ClientId).SendAsync("onBalance", balance);
		}

		public static void BroadcastBalance(int clientId, ApiBalance balance)
		{
			if (balance.Balances != null)
			{
				balance.AvailableBalance = Math.Floor(balance.Balances.Where(x => x.TypeId != (int)AccountTypes.ClientBonusBalance &&
																	x.TypeId != (int)AccountTypes.ClientCompBalance &&
																	x.TypeId != (int)AccountTypes.ClientCoinBalance)
														.Sum(x => x.Balance) * 100) / 100;
			}
			BaseHub.CurrentContext.Clients.Group("Client_" + clientId).SendAsync("onBalance", balance);
		}

		public static void BroadcastLogout(string connectionId)
		{
			BaseHub.CurrentContext.Clients.Client(connectionId).SendAsync("onLogout");
		}

		public static void BroadcastDepositLimit(LimitInfo info)
		{
			BaseHub.CurrentContext.Clients.Group("Client_" + info.ClientId).SendAsync("onDepositLimit", info);
		}

		public static void BroadcastBetLimit(LimitInfo info)
		{
			BaseHub.CurrentContext.Clients.Group("Client_" + info.ClientId).SendAsync("onBetLimit", info);
		}
	}
}