using System.Configuration;
using System.Web;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using System.Threading;
using IqSoft.CP.BLL.Caching;
using System;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.AdminWebApi.Hubs;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Web.Http;

namespace IqSoft.CP.AdminWebApi
{
    public class WebApiApplication : HttpApplication
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
            qParams.Add("ProjectId", ((int)ProjectTypes.AdminWebApi).ToString());
            _jobConnection = new HubConnection(ConfigurationManager.AppSettings["JobHostAddress"], qParams);
             JobHubProxy = _jobConnection.CreateHubProxy("BaseHub");
            _jobConnection.Closed += () => { Timer.Change(5000, 5000); };

            JobHubProxy.On<string, object, TimeSpan>("onUpdateCacheItem", (key, newValue, timeSpan) =>
            {
                CacheManager.UpdateCacheItem(key, newValue, timeSpan);
            });
            JobHubProxy.On<int>("onUpdateProduct", (productId) =>
            {
                CacheManager.DeleteProductFromCache(productId);
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
                CacheManager.RemoveKeysFromCache(data);
            });
            JobHubProxy.On<int>("onRemoveClient", (clientId) =>
            {
                CacheManager.RemoveClientFromCache(clientId);
            });
            JobHubProxy.On<int>("onPaymentRequst", (paymentRequestId) =>
            {               
                BaseHub.PaymentRequst(paymentRequestId);
            });
            JobHubProxy.On<ApiWin>("BroadcastBalance", (balance) =>
            {
                WebSiteHub.BroadcastBalance(balance);
            });
            JobHubProxy.On<int>("onNotificationsCount", (userId) =>
            {
                BaseHub.BroadcastNotificationsCount(userId, CacheManager.GetUserNotificationsCount(userId).Count);
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
                catch
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