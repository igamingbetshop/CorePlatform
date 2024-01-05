using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.WebSiteWebApi.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IqSoft.CP.WebSiteWebApi.Hubs
{
	public class BaseHub : Hub
	{
        public static IHubContext<BaseHub> CurrentContext;

        public static ConcurrentDictionary<string, ApiIdentity> ConnectedClients = new ConcurrentDictionary<string, ApiIdentity>();

        public BaseHub(IHubContext<BaseHub> hubContext) : base()
        {
            CurrentContext = hubContext;
        }

        #region Hub Implementation

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
            var ip = Constants.DefaultIp;
            if (Context.GetHttpContext().Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues svIp))
                ip = svIp.ToString();
            var partnerId = Context.GetHttpContext().Request.Query["PartnerId"];
            var token = Context.GetHttpContext().Request.Query["Token"];
            var languageId = Context.GetHttpContext().Request.Query["LanguageId"];
            var timeZone = Context.GetHttpContext().Request.Query["TimeZone"];
            await BaseHub.CurrentContext.Groups.AddToGroupAsync(Context.ConnectionId, "Partner_" + partnerId);
            var apiIdentity = new ApiIdentity { PartnerId = Convert.ToInt32(partnerId), LanguageId = languageId, TimeZone = Convert.ToDouble(timeZone) };
            if (!string.IsNullOrEmpty(token))
            {
                var client = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(Convert.ToInt32(partnerId), "GetClientByToken",
                    new RequestBase { Token = token, LanguageId = apiIdentity.LanguageId });
                if (client.ResponseCode == Constants.SuccessResponseCode)
                {
                    await BaseHub.CurrentContext.Groups.AddToGroupAsync(Context.ConnectionId, "Client_" + client.Id);
                    apiIdentity.ClientId = client.Id;
                    apiIdentity.Token = token;

                    var balance = MasterCacheIntegration.SendMasterCacheRequest<ApiBalance>(client.PartnerId, 
                        "getclientbalance", new RequestBase { 
                        Token = token,
                        ClientId = client.Id,
                        PartnerId = client.PartnerId,
                        RequestData = client.CurrencyId
                    });
                    BroadcastService.BroadcastBalance(client.Id, balance);
                }
            }
            ConnectedClients.TryRemove(Context.ConnectionId, out ApiIdentity oldIdentity);
            ConnectedClients.TryAdd(Context.ConnectionId, apiIdentity);
        }

        public override async Task OnDisconnectedAsync(Exception e)
		{
			try
			{
                ConnectedClients.TryRemove(Context.ConnectionId, out ApiIdentity identityApi);
                await base.OnDisconnectedAsync(e);
			}
			catch (Exception)
			{
                await base.OnDisconnectedAsync(e);
			}
		}

        #endregion
	}
}