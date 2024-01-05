using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using log4net;
using log4net.Config;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Web.Http;

namespace IqSoft.CP.MasterCacheWebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static ILog DbLogger { get; private set; }
        private static HubConnection _jobConnection;
        public static IHubProxy JobHubProxy;
        public static Timer Timer;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            XmlConfigurator.Configure();
            DbLogger = LogManager.GetLogger("DbLogAppender");
            var qParams = new Dictionary<string, string>();
            qParams.Add("ProjectId", ((int)ProjectTypes.MasterCache).ToString());
            _jobConnection = new HubConnection(ConfigurationManager.AppSettings["JobHostAddress"], qParams);
            JobHubProxy = _jobConnection.CreateHubProxy("BaseHub");
            _jobConnection.Closed += () => { Timer.Change(5000, 5000); };

            JobHubProxy.On<int>("onClientDepositWithBonus", (clientId) =>
            {
                CacheManager.RemoveClientBalance(clientId);
                CacheManager.RemoveClientDepositCount(clientId);
                CacheManager.RemoveClientActiveBonus(clientId);
            });
            JobHubProxy.On<int>("onClientDeposit", (clientId) =>
            {
                CacheManager.RemoveClientBalance(clientId);
                CacheManager.RemoveClientDepositCount(clientId);
                CacheManager.RemoveTotalDepositAmount(clientId);
            });
            JobHubProxy.On<int>("onClientBonus", (clientId) =>
            {
                CacheManager.RemoveClientBalance(clientId);
                CacheManager.RemoveClientActiveBonus(clientId);
            });
            JobHubProxy.On<int, string>("onLoginClient", (clientId, ip) =>
            {
                CacheManager.RemoveClientFromCache(clientId);
                CacheManager.UpdateClientLastLoginIp(clientId, ip);
                CacheManager.RemoveClientNotAwardedCampaigns(clientId);
                CacheManager.RemoveClientFailedLoginCount(clientId);
            });
            JobHubProxy.On<string, object, TimeSpan>("onUpdateCacheItem", (key, newValue, timeSpan) =>
            {
                CacheManager.UpdateCacheItem(key, newValue, timeSpan);
            });
            JobHubProxy.On<int>("onUpdateClientFailedLoginCount", (clientId) =>
            {
                CacheManager.UpdateClientFailedLoginCount(clientId);
            });
            JobHubProxy.On<int>("onUpdateProduct", (productId) =>
            {
                CacheManager.UpdateProductById(productId);
            });
            JobHubProxy.On<int, long, int, int>("onUpdateProductLimit", (objectTypeId, objectId, limitTypeId, productId) =>
            {
                CacheManager.RemoveProductLimit(objectTypeId, objectId, limitTypeId, productId);
            });
            JobHubProxy.On<string>("onRemoveKeyFromCache", (data) =>
            {
                CacheManager.RemoveFromCache(data);
            });
            JobHubProxy.On<string>("onRemoveKeysFromCache", (data) =>
            {
                CacheManager.RemoveFromCache(data);
            });
            JobHubProxy.On<int>("onRemovePartnerProductSettings", (partnerId) =>
            {
                CacheManager.RemovePartnerProductSettings(partnerId);
            });
            JobHubProxy.On<int, int, string>("onRemoveComplimentaryPointRate", (partnerId, productId, currencyId) =>
            {
                CacheManager.RemoveComplimentaryPointRate(partnerId, productId, currencyId);
            });
            JobHubProxy.On<int, int>("onRemoveCommentTemplateFromCache", (partnerId, commentType) =>
            {
                CacheManager.RemoveCommentTemplateFromCache(partnerId, commentType);
            });
            JobHubProxy.On<int, int>("onRemoveBanners", (partnerId, type) =>
             {
                 CacheManager.RemoveBanners(partnerId, type);
             });
            JobHubProxy.On<int>("onRemoveClient", (clientId) =>
            {
                CacheManager.RemoveClientFromCache(clientId);
            });
            JobHubProxy.On<int>("ExpireClientPlatformSessions", (id) =>
            {
                BroadcastListener.ExpireClientPlatformSessions();
            });
            JobHubProxy.On<int>("ExpireClientProductSessions", (id) =>
            {
                BroadcastListener.ExpireClientProductSessions();
            });

            Timer = new Timer(Reconnect, null, 5000, 5000);
        }

        private void Reconnect(object sender)
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            bool activeConnections = true;
            if (_jobConnection.State != ConnectionState.Connected)
            {
                try
                {
                    _jobConnection.Start().Wait();
                }
                catch(Exception)
                {
                    activeConnections = false;
                }
            }

            if (!activeConnections)
            {
                Timer.Change(5000, 5000);
            }
        }
    }
}
