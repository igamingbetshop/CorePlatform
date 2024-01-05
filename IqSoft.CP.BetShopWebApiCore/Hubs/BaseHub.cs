using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApiCore;
using IqSoft.CP.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace IqSoft.CP.BetShopWebApi.Hubs
{
	public class BaseHub : Hub
	{
		public static IHubContext<BaseHub> CurrentContext;

		public BaseHub(IHubContext<BaseHub> hubContext) : base()
		{
			CurrentContext = hubContext;
		}

		public override async Task OnConnectedAsync()
		{
			try
			{
				Connect();
				await base.OnConnectedAsync();
			}
			catch 
			{
				await base.OnConnectedAsync();
			}
		}

		private void Connect()
		{
			var context = Context.GetHttpContext();
			string partnerId = context.Request.Query[QueryStringParams.PartnerId];
			string cashDeskId = context.Request.Query[QueryStringParams.CashDeskId];
			try
			{
				var ip = Constants.DefaultIp;
				if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();

				if (string.IsNullOrWhiteSpace(partnerId) || !int.TryParse(partnerId, out int parsedPartnerId))
					throw new Exception(Constants.Errors.PartnerNotFound.ToString());
				if (parsedPartnerId == Constants.MainPartnerId)
					return;
				string languageId = context.Request.Query[QueryStringParams.LanguageId];
				if (string.IsNullOrWhiteSpace(languageId))
				{
					languageId = Constants.DefaultLanguageId;
				}
				var token = context.Request.Query[QueryStringParams.Token];
				if (string.IsNullOrWhiteSpace(cashDeskId) || !int.TryParse(cashDeskId, out int parsedCashDeskId))
					throw new Exception(Constants.Errors.CashDeskNotFound.ToString());

				var timeZone = Convert.ToDouble(context.Request.Query[QueryStringParams.TimeZone]);

				var session = PlatformIntegration.GetCashierSessionByToken(new GetCashierSessionInput
				{
					TimeZone = timeZone,
					LanguageId = languageId,
					PartnerId = parsedPartnerId,
					SessionToken = token
				});
				if (session == null)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());

				if (Program.Clients.ContainsKey(token))
				{
					if (!Program.Clients[token].ConnectionIds.Contains(Context.ConnectionId))
						Program.Clients[token].ConnectionIds.Add(Context.ConnectionId);
				}
				else
				{
					Program.Clients.TryAdd(token, new CashierIdentity
					{
						ConnectionIds = new List<string> { Context.ConnectionId },
						TimeZone = timeZone,
						LanguageId = languageId,
						PartnerId = parsedPartnerId,
						CashDeskId = parsedCashDeskId,
						CashierId = session.UserId,
						BetShopId = session.BetShopId
					});
				}
			}
			catch (Exception e)
			{
				Program.LogWriter.Error(string.Format("{0}_{1}", partnerId, cashDeskId), e);
			}
		}

		public override async Task OnDisconnectedAsync(Exception e)
		{
			try
			{
				OnDisconnected();
				await base.OnDisconnectedAsync(e);
			}
			catch (Exception)
			{
				await base.OnDisconnectedAsync(e);
			}
		}

		private void OnDisconnected()
		{
			var token = Context.GetHttpContext().Request.Query[QueryStringParams.Token];
			if (Program.Clients.ContainsKey(token))
			{
				Program.Clients[token].ConnectionIds.Remove(Context.ConnectionId);
				if (!Program.Clients[token].ConnectionIds.Any())
					Program.Clients.TryRemove(token, out _);
			}
		}
	}
}