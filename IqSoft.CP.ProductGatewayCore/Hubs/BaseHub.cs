using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IqSoft.CP.ProductGateway.Hubs
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
				await Connect();
				await base.OnConnectedAsync();
			}
			catch (Exception e)
			{
				Program.DbLogger.Error(e.Message);
				await base.OnConnectedAsync();
			}
		}

		private async Task Connect()
		{
			await BaseHub.CurrentContext.Groups.AddToGroupAsync(Context.ConnectionId, "WebSiteWebApi");
		}

		public override async Task OnDisconnectedAsync(Exception e)
		{
			try
			{
				await BaseHub.CurrentContext.Groups.RemoveFromGroupAsync(Context.ConnectionId, "WebSiteWebApi");
				await base.OnDisconnectedAsync(e);
			}
			catch (Exception)
			{
				await base.OnDisconnectedAsync(e);
			}
		}
	}
}