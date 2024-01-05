using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.Common;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace IqSoft.CP.BetShopWebApi.Hubs
{
	[HubName("BaseHub")]
	public class BaseHub : Hub
	{
		public override Task OnConnected()
		{
			Connect();
			return base.OnConnected();
		}

		public override Task OnReconnected()
		{
			Connect();
			return base.OnReconnected();
		}

		private void Connect()
		{
			string partnerId = Context.QueryString[QueryStringParams.PartnerId];
			string cashDeskId = Context.QueryString[QueryStringParams.CashDeskId];
			try
			{
				var ip = HttpContext.Current == null ? Constants.DefaultIp : HttpContext.Current.Request.UserHostAddress;
				int parsedPartnerId = 0;
				if (string.IsNullOrWhiteSpace(partnerId) || !int.TryParse(partnerId, out parsedPartnerId))
					throw new Exception(Constants.Errors.PartnerNotFound.ToString());
				if (parsedPartnerId == Constants.MainPartnerId)
					return;
				string languageId = Context.QueryString[QueryStringParams.LanguageId];
				if (string.IsNullOrWhiteSpace(languageId))
				{
					languageId = Constants.DefaultLanguageId;
				}
				var token = Context.QueryString[QueryStringParams.Token];
				int parsedCashDeskId = 0;
				if (string.IsNullOrWhiteSpace(cashDeskId) || !int.TryParse(cashDeskId, out parsedCashDeskId))
					throw new Exception(Constants.Errors.CashDeskNotFound.ToString());

				var timeZone = Convert.ToDouble(Context.QueryString[QueryStringParams.TimeZone]);

				var session = PlatformIntegration.GetCashierSessionByToken(new GetCashierSessionInput
				{
					TimeZone = timeZone,
					LanguageId = languageId,
					PartnerId = parsedPartnerId,
					SessionToken = token
				});
				if (session == null)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());

				if (WebApiApplication.Clients.ContainsKey(token))
				{
					if (!WebApiApplication.Clients[token].ConnectionIds.Contains(Context.ConnectionId))
						WebApiApplication.Clients[token].ConnectionIds.Add(Context.ConnectionId);
				}
				else
				{
					WebApiApplication.Clients.TryAdd(token, new CashierIdentity
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
				WebApiApplication.LogWriter.Error(string.Format("{0}_{1}", partnerId, cashDeskId), e);
			}
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			var token = Context.QueryString[QueryStringParams.Token];
			if (WebApiApplication.Clients.ContainsKey(token))
			{
				WebApiApplication.Clients[token].ConnectionIds.Remove(Context.ConnectionId);
				if (!WebApiApplication.Clients[token].ConnectionIds.Any())
					WebApiApplication.Clients.TryRemove(token, out CashierIdentity cashierIdentity);
			}
			return base.OnDisconnected(stopCalled);
		}
	}
}