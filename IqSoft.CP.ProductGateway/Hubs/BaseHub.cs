using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace IqSoft.CP.ProductGateway.Hubs
{
	[HubName("BaseHub")]
	public class BaseHub : Hub
	{
		public override Task OnConnected()
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
			context.Groups.Add(Context.ConnectionId, "WebSiteWebApi");
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
			context.Groups.Remove(Context.ConnectionId, "WebSiteWebApi");
			return base.OnDisconnected(true);
		}
	}
}