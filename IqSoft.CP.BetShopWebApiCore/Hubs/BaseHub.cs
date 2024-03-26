using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IqSoft.CP.BetShopWebApi.Hubs
{
	public class BaseHub : Hub
	{
		public static IHubContext<BaseHub> CurrentContext;

		public static ConcurrentDictionary<string, CashierIdentity> ConnectedClients = new ConcurrentDictionary<string, CashierIdentity>();

		public BaseHub(IHubContext<BaseHub> hubContext) : base()
		{
			CurrentContext = hubContext;
		}

		public override async Task OnConnectedAsync()
		{
			try
			{
				await Connect();
				await base.OnConnectedAsync();
			}
			catch (Exception e)
			{
				Log.Error(e, "ERROR_OnConnectedAsync");
				await base.OnConnectedAsync();
			}
		}

		private async Task Connect()
		{
			string partnerId = string.Empty, cashDeskId = string.Empty;
			try
			{
				partnerId = Context.GetHttpContext().Request.Query[QueryStringParams.PartnerId];
				cashDeskId = Context.GetHttpContext().Request.Query[QueryStringParams.CashDeskId];
				string languageId = Context.GetHttpContext().Request.Query[QueryStringParams.LanguageId];
				var timeZone = Convert.ToDouble(Context.GetHttpContext().Request.Query[QueryStringParams.TimeZone]);
				var token = Context.GetHttpContext().Request.Query[QueryStringParams.Token];

				if (string.IsNullOrWhiteSpace(partnerId) || !int.TryParse(partnerId, out int parsedPartnerId))
					throw new Exception($"Code: {Constants.Errors.PartnerNotFound}, Description: {nameof(Constants.Errors.PartnerNotFound)}");
				if (parsedPartnerId == Constants.MainPartnerId)
					return;
				var ip = Constants.DefaultIp;
				if (Context.GetHttpContext().Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues svIp))
					ip = svIp.ToString();

				if (string.IsNullOrWhiteSpace(languageId))
					languageId = Constants.DefaultLanguageId;
				if (string.IsNullOrWhiteSpace(cashDeskId) || !int.TryParse(cashDeskId, out int parsedCashDeskId))
					throw new Exception(Constants.Errors.CashDeskNotFound.ToString());
				var requestInput = new PlatformRequestBase
				{
					TimeZone = timeZone,
					LanguageId = languageId,
					PartnerId = parsedPartnerId,
					Token = token
				};
				var resp = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetCashierSessionByToken);
				if (resp.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception($"Code: {resp.ResponseCode}, Description: {resp.Description}");
				var session = JsonConvert.DeserializeObject<AuthorizationOutput>(JsonConvert.SerializeObject(resp.ResponseObject));

				if (ConnectedClients.ContainsKey(token))
				{
					if (!ConnectedClients[token].ConnectionIds.Contains(Context.ConnectionId))
						ConnectedClients[token].ConnectionIds.Add(Context.ConnectionId);
				}
				else
				{
					ConnectedClients.TryAdd(token, new CashierIdentity
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
				await BaseHub.CurrentContext.Groups.AddToGroupAsync(Context.ConnectionId, "Partner_" + partnerId);

			}
			catch (Exception e)
			{
				Log.Error(string.Format("{0}_{1}", partnerId, cashDeskId), e);
			}
		}

		public override async Task OnDisconnectedAsync(Exception e)
		{
			try
			{
				var token = Context.GetHttpContext().Request.Query[QueryStringParams.Token];
				if (ConnectedClients.ContainsKey(token))
				{
					ConnectedClients[token].ConnectionIds.Remove(Context.ConnectionId);
					if (!ConnectedClients[token].ConnectionIds.Any())
						ConnectedClients.TryRemove(token, out CashierIdentity cashierIdentity);
				}
				await base.OnDisconnectedAsync(e);
			}
			catch (Exception)
			{
				await base.OnDisconnectedAsync(e);
			}
		}
    }
}