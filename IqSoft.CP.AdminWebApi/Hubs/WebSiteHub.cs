using IqSoft.CP.Common.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace IqSoft.CP.AdminWebApi.Hubs
{
	[HubName("WebSiteHub")]
	public class WebSiteHub : Hub
	{
		public override Task OnConnected()
		{
			//var partnerId = Context.QueryString[QueryStringParams.PartnerId];
			var context = GlobalHost.ConnectionManager.GetHubContext<WebSiteHub>();
			context.Groups.Add(Context.ConnectionId, "WebSiteWebApi");

			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			//var partnerId = Context.QueryString[QueryStringParams.PartnerId];
			var context = GlobalHost.ConnectionManager.GetHubContext<WebSiteHub>();
			context.Groups.Remove(Context.ConnectionId, "WebSiteWebApi");

			return base.OnDisconnected(true);
		}
	}
}