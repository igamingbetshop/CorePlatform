using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class BroadcastListener
    {
        public static void ExpireClientPlatformSessions()
        {
            var sessionsToRemove = new List<BllClientSession>();
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var platformSessionClientIds = (from cs in db.ClientSessions
                                                where cs.ClientId > 0 && cs.ProductId == (int)Constants.PlatformProductId && cs.State == (int)SessionStates.Active
                                                group cs by cs.Client.PartnerId into y
                                                select new { PartnerId = y.Key, ClientIds = y.Select(x => x.ClientId) }).ToList();

                foreach (var dbSessionPartner in platformSessionClientIds)
                {
                    var partner = db.Partners.FirstOrDefault(x => x.Id == dbSessionPartner.PartnerId);
                    foreach (var dbSessionClientId in dbSessionPartner.ClientIds)
                    {
                        var cacheSession = CacheManager.GetClientPlatformSession(dbSessionClientId, null, false);
                        if (cacheSession != null)
                        {
                            var clientSessionLimit = CacheManager.GetClientSettingByName(cacheSession.ClientId, ClientSettings.SessionLimit);
                            var clientSessionSystemLimit = CacheManager.GetClientSettingByName(cacheSession.ClientId, ClientSettings.SystemSessionLimit);

                            var value = (clientSessionLimit == null || clientSessionLimit.Id == 0 || clientSessionLimit.NumericValue == null ? -1 : clientSessionLimit.NumericValue.Value);
                            if (clientSessionSystemLimit != null && clientSessionSystemLimit.Id > 0 && clientSessionSystemLimit.NumericValue != null)
                            {
                                if (value == -1) value = clientSessionSystemLimit.NumericValue.Value;
                                else value = Math.Min(value, clientSessionSystemLimit.NumericValue.Value);
                            }

                            int? logutReason = null;
                            if (cacheSession.LastUpdateTime < currentTime.AddMinutes(-partner.ClientSessionExpireTime))
                                logutReason = (int)LogoutTypes.Expired;
                            else if (value > 0 && cacheSession.StartTime < currentTime.AddMinutes(-(double)value))
                                logutReason = (int)LogoutTypes.SessionLimit;

                            if (logutReason.HasValue)
                            {
                                sessionsToRemove.Add(cacheSession);
                                db.ClientSessions.Where(x => x.Id == cacheSession.Id).UpdateFromQuery(x => new ClientSession
                                {
                                    LastUpdateTime = cacheSession.LastUpdateTime,
                                    State = (int)SessionStates.Inactive,
                                    EndTime = currentTime,
                                    LogoutType = logutReason.Value
                                });
                                db.Clients.Where(x => x.Id == dbSessionClientId).UpdateFromQuery(x => new Client { LastSessionId = cacheSession.Id });
                            }
                        }
                    }
                }
            }
            foreach (var s in sessionsToRemove)
            {
                CacheManager.RemoveClientPlatformSession(s.ClientId);
                CacheManager.RemoveClientProductSession(s.Token, Constants.PlatformProductId);
                Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, s.ClientId));
                Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, s.Token));

                CacheManager.RemoveClientFromCache(s.ClientId);
                Helpers.InvokeMessage("RemoveClient", s.ClientId);
            }
        }

        public static void ExpireClientProductSessions()
        {
            var sessionsToRemove = new List<BllClientSession>();
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var productSessionTokens = (from cs in db.ClientSessions
                                            where cs.ClientId > 0 && cs.ProductId != (int)Constants.PlatformProductId && cs.State == (int)SessionStates.Active
                                            group cs by cs.ProductId into y
                                            select new { ProductId = y.Key, Tokens = y.Select(x => x.Token) }).ToList();
                foreach (var dbSessionProduct in productSessionTokens)
                {
                    var product = CacheManager.GetProductById(dbSessionProduct.ProductId);
                    var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                    foreach (var dbSession in dbSessionProduct.Tokens)
                    {
                        var cacheSession = CacheManager.GetClientSessionByToken(dbSession, dbSessionProduct.ProductId, false);
                        var expireTime = currentTime.AddMinutes(-provider.SessionExpireTime.Value);

                        if (cacheSession != null && cacheSession.LastUpdateTime < expireTime)
                        {
                            sessionsToRemove.Add(new BllClientSession { ProductId = product.Id, Token = dbSession });
                            db.ClientSessions.Where(x => x.Id == cacheSession.Id).UpdateFromQuery(x => new ClientSession
                            {
                                LastUpdateTime = cacheSession.LastUpdateTime,
                                State = (int)SessionStates.Inactive,
                                EndTime = currentTime, 
                                LogoutType = (int)LogoutTypes.Expired
                            });
                        }
                    }
                }
            }
            foreach (var s in sessionsToRemove)
            {
                CacheManager.RemoveClientProductSession(s.Token, s.ProductId);
                Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_0", Constants.CacheItems.ClientSessions, s.Token));
                Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, s.Token, s.ProductId));
            }
        }
    }
}