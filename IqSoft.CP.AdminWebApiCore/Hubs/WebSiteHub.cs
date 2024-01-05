using IqSoft.CP.Common.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using IqSoft.CP.AdminWebApiCore;

namespace IqSoft.CP.AdminWebApi.Hubs
{
	public class WebSiteHub : Hub
	{
		public static IHubContext<WebSiteHub> CurrentContext;
		public WebSiteHub(IHubContext<WebSiteHub> hubContext) : base()
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
				Program.DbLogger.Error(e.Message);
				await base.OnConnectedAsync();
			}
		}

		public override async Task OnDisconnectedAsync(Exception e)
		{
			try
			{
				if (CurrentContext != null)
					await Disconnect(e);
				await base.OnDisconnectedAsync(e);
			}
			catch (Exception)
			{
				await base.OnDisconnectedAsync(e);
			}
		}
		public async Task Connect()
		{
			var partnerId = Context.GetHttpContext().Request.Query[QueryStringParams.PartnerId];
			await CurrentContext.Groups.AddToGroupAsync(Context.ConnectionId, "WebSiteWebApi_" + partnerId);
		}

		public async Task Disconnect(Exception e)
		{
			var partnerId = Context.GetHttpContext().Request.Query[QueryStringParams.PartnerId];
			await CurrentContext.Groups.RemoveFromGroupAsync(Context.ConnectionId, "WebSiteWebApi_" + partnerId);
		}
	}
}