using IqSoft.CP.JobService.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IqSoft.CP.JobService.Hubs
{
	public class BaseHub : Hub
	{
		public static IHubContext<BaseHub> CurrentContext;
		public static ConcurrentDictionary<string, MasterCacheIdentity> Caches = new ConcurrentDictionary<string, MasterCacheIdentity>();
		public BaseHub(IHubContext<BaseHub> hubContext) : base()
		{
			CurrentContext = hubContext;
		}
		public override async Task OnConnectedAsync()
		{
			await Connect();
			await base.OnConnectedAsync();
		}

		private async Task Connect()
		{
			var projectId = Context.GetHttpContext().Request.Query["ProjectId"];
			await BaseHub.CurrentContext.Groups.AddToGroupAsync(Context.ConnectionId, "BaseHub");
			Caches.TryRemove(Context.ConnectionId, out MasterCacheIdentity item);
			Caches.TryAdd(Context.ConnectionId, new MasterCacheIdentity { ProjectId = Convert.ToInt32(projectId) });
		}

		public override async Task OnDisconnectedAsync(Exception e)
		{
			await BaseHub.CurrentContext.Groups.RemoveFromGroupAsync(Context.ConnectionId, "BaseHub");
			Caches.TryRemove(Context.ConnectionId, out MasterCacheIdentity item);
			await base.OnDisconnectedAsync(e);
		}

		public void UpdateCacheItem(string key, object newValue, TimeSpan timeSpan)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					CurrentContext.Clients.Client(item.Key).SendAsync("onUpdateCacheItem", key, newValue, timeSpan);
			}
		}
		public void UpdateClientFailedLoginCount(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onUpdateClientFailedLoginCount", clientId);
			}
		}
		public void UpdateProduct(int productId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onUpdateProduct", productId);
			}
		}

		public void UpdateProductLimit(int objectTypeId, long objectId, int limitTypeId, int productId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onUpdateProductLimit", objectTypeId, objectId, limitTypeId, productId);
			}
		}

		public void RemoveKeyFromCache(string key)
		{

			foreach (var item in Caches)
			{

				if (item.Key != Context.ConnectionId)
				{
					Clients.Client(item.Key).SendAsync("onRemoveKeyFromCache", key);
				}
			}
		}

		public void RemoveBanners(string key)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onRemoveBanners", key);
			}
		}
		public void RemoveClient(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onRemoveClient", clientId);
			}
		}

		public void ClientDeposit(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onClientDeposit", clientId);
			}
		}

		public void ClientDepositWithBonus(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onClientDepositWithBonus", clientId);
			}
		}

		public void ClientBonus(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onClientBonus", clientId);
			}
		}
		public void LoginClient(int clientId, string ip)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onLoginClient", clientId, ip);
			}
		}

		public void PaymentRequst(int paymentRequestId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).SendAsync("onPaymentRequst", paymentRequestId);
			}
		}
	}
}
