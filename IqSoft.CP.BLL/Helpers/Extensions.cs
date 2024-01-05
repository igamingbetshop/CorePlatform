using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace IqSoft.CP.BLL.Helpers
{
    public static class Extensions
    {
        public static BllClientSession ToBllClientSession(this ClientSession session, string clientCurrencyId = "")
        {
            return new BllClientSession
            {
                Id = session.Id,
                ClientId = session.ClientId,
                LanguageId = session.LanguageId,
                Ip = session.Ip,
                Country = session.Country,
                Token = session.Token,
                ProductId = session.ProductId,
                DeviceType = session.DeviceType,
                StartTime = session.StartTime,
                LastUpdateTime = session.LastUpdateTime ?? session.StartTime,
                State = session.State,
                CurrentPage = session.CurrentPage,
                ParentId = session.ParentId,
                CurrencyId = clientCurrencyId,
                ExternalToken = session.ExternalToken
            };
        }
    }

    public static class DbSetExtensions
    {
        public static T AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }
    }

}
