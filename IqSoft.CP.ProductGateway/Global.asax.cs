using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.ProductGateway.Controllers;
using log4net;
using log4net.Config;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway
{
    public class WebApiApplication : HttpApplication
    {
        public static ILog DbLogger { get; private set; }
        private static HubConnection _jobConnection;
        public static IHubProxy JobHubProxy;
        public static Timer Timer;

        public static string BetShopConnectionUrl { get; set; }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            XmlConfigurator.Configure();
            DbLogger = LogManager.GetLogger("DbLogAppender");
            BetShopConnectionUrl = ConfigurationManager.AppSettings["BetShopConnectionUrl"];
            var qParams = new Dictionary<string, string>() { { "ProjectId", ((int)ProjectTypes.ProductGateway).ToString() } };
            _jobConnection = new HubConnection(ConfigurationManager.AppSettings["JobHostAddress"], qParams);
            JobHubProxy = _jobConnection.CreateHubProxy("BaseHub");
            _jobConnection.Closed += () => { Timer.Change(5000, 5000); };
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
                CacheManager.DeleteProductFromCache(productId);
            });
            JobHubProxy.On<int, long, int, int>("onUpdateProductLimit", (objectTypeId, objectId, limitTypeId, productId) =>
            {                
                CacheManager.RemoveProductLimit(objectTypeId, objectId, limitTypeId, productId);
            });
            JobHubProxy.On<string>("onUpdateWhitelistedIps", (data) =>
            {
                ReDefineWhitelistedIps(data.Replace(Constants.CacheItems.WhitelistedIps, string.Empty));
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
            Timer = new Timer(Reconnect, null, 5000, 5000);
        }

        private void ReDefineWhitelistedIps(string providerName)
        {
            try
            {
                var nSpace = GetType().BaseType.Namespace;
                var type = Type.GetType($"{nSpace}.Controllers.{providerName}Controller");
                if (type != null)
                {
                    var field = type.GetField(Constants.CacheItems.WhitelistedIps, BindingFlags.Public | BindingFlags.Static);
                    field.SetValue(null, CacheManager.GetProviderWhitelistedIps(providerName));
                }
            }
            catch (Exception ex)
            {
                DbLogger.Info(ex);
            }
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
