using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static IHubContext<BaseHub> CurrentContext;
        public BaseHub(IHubContext<BaseHub> hubContext) : base()
        {
            CurrentContext = hubContext;
        }
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
			string partnerId = string.Empty, cashDeskId = string.Empty;
			try
			{
				partnerId = Context.QueryString[QueryStringParams.PartnerId];
				cashDeskId = Context.QueryString[QueryStringParams.CashDeskId];
				string languageId = Context.QueryString[QueryStringParams.LanguageId];
				var timeZone = Convert.ToDouble(Context.QueryString[QueryStringParams.TimeZone]);
				var token = Context.QueryString[QueryStringParams.Token];

				if (string.IsNullOrWhiteSpace(partnerId) || !int.TryParse(partnerId, out int parsedPartnerId))
					throw new Exception($"Code: {Constants.Errors.PartnerNotFound}, Description: {nameof(Constants.Errors.PartnerNotFound)}");
				if (parsedPartnerId == Constants.MainPartnerId)
					return;
				var ip = HttpContext.Current == null ? Constants.DefaultIp : HttpContext.Current.Request.UserHostAddress;

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
                BaseHub.CurrentContext.Groups.Add(Context.ConnectionId, "Partner_" + partnerId);
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