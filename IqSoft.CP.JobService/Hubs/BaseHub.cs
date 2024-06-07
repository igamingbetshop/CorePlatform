using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.JobService.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace IqSoft.CP.JobService.Hubs
{
	[HubName("BaseHub")]
	public class BaseHub : Hub
	{
		public static ConcurrentDictionary<string, MasterCacheIdentity> Caches = new ConcurrentDictionary<string, MasterCacheIdentity>();
		public static readonly dynamic _connectedClients = GlobalHost.ConnectionManager.GetHubContext<BaseHub>().Clients;
        public override Task OnConnected()
        {
            var projectId = Context.QueryString["ProjectId"];
            var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
            context.Groups.Add(Context.ConnectionId, "BaseHub");

            Caches.TryRemove(Context.ConnectionId, out MasterCacheIdentity item);
            Caches.TryAdd(Context.ConnectionId, new MasterCacheIdentity { ProjectId = Convert.ToInt32(projectId) });
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
			context.Groups.Remove(Context.ConnectionId, "BaseHub");
            Caches.TryRemove(Context.ConnectionId, out MasterCacheIdentity item);
			return base.OnDisconnected(true);
		}

        public static void BroadcastBalance(int clientId)
        {
            var balance = CacheManager.GetClientCurrentBalance(clientId);

            _connectedClients.Group("BaseHub").BroadcastBalance(new ApiWin
            {
                ClientId = clientId,
                ApiBalance = new ApiBalance
                {
                    AvailableBalance = balance.AvailableBalance,
                    Balances = balance.Balances.Select(x => new ApiAccount
                    {
                        Id = x.Id,
                        TypeId = x.TypeId,
                        CurrencyId = x.CurrencyId,
                        Balance = x.Balance,
                        BetShopId = x.BetShopId,
                        PaymentSystemId = x.PaymentSystemId
                    }).ToList()
                }
            });
        }

        public void UpdateCacheItem(string key, object newValue, TimeSpan timeSpan)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onUpdateCacheItem(key, newValue, timeSpan);
			}
		}
		public void UpdateClientFailedLoginCount(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onUpdateClientFailedLoginCount(clientId);
			}
		}
		public void UpdateProduct(int productId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onUpdateProduct(productId);
			}
		}

		public void UpdateWhitelistedIps(string data)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onUpdateWhitelistedIps(data);
			}
		}

		public void UpdateProductLimit(int objectTypeId, long objectId, int limitTypeId, int productId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onUpdateProductLimit(objectTypeId, objectId, limitTypeId, productId);
			}
		}

		public void RemoveKeyFromCache(string key)
		{

			foreach (var item in Caches)
			{

				if (item.Key != Context.ConnectionId)
				{
					Clients.Client(item.Key).onRemoveKeyFromCache(key);
				}
			}
		}

		public void RemoveKeysFromCache(string key)
		{

			foreach (var item in Caches)
			{

				if (item.Key != Context.ConnectionId)
				{
					Clients.Client(item.Key).onRemoveKeysFromCache(key);
				}
			}
		}

		public void RemovePartnerProductSettings(int partnerId)
		{
			foreach (var item in Caches)
			{

				if (item.Key != Context.ConnectionId)
				{
					Clients.Client(item.Key).onRemovePartnerProductSettings(partnerId);
				}
			}
		}

        public void RemoveComplimentaryPointRate(int partnerId, int productId, string currencyId)
        {

            foreach (var item in Caches)
            {

                if (item.Key != Context.ConnectionId)
                {
                    Clients.Client(item.Key).onRemoveComplimentaryPointRate(partnerId, productId, currencyId);
                }
            }
        }

        public void RemoveComplimentaryPointRate(int partnerId, int commentType)
        {

            foreach (var item in Caches)
            {

                if (item.Key != Context.ConnectionId)
                {
                    Clients.Client(item.Key).onRemoveCommentTemplateFromCache(partnerId, commentType);
                }
            }
        }

        public void RemoveBanners(string key)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onRemoveBanners(key);
			}
		}
		public void RemoveClient(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onRemoveClient(clientId);
			}
		}

		public void ClientDeposit(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onClientDeposit(clientId);
			}
		}

		public void ClientDepositWithBonus( int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onClientDepositWithBonus(clientId);
			}
		}

		public void ClientBonus(int clientId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onClientBonus(clientId);
			}
		}
		public void LoginClient(int clientId, string ip)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onLoginClient(clientId, ip);
			}
		}

		public void PaymentRequst(int paymentRequestId)
		{
			foreach (var item in Caches)
			{
				if (item.Key != Context.ConnectionId)
					Clients.Client(item.Key).onPaymentRequst(paymentRequestId);
			}
		}
	}
}
