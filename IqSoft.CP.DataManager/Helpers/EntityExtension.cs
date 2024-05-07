using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace IqSoft.CP.DataManager.Helpers
{
    public static class EntityExtension
    {
        public static void AddOrUpdate<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var dbItem = dbSet.FirstOrDefault(predicate);
            if (dbItem == null)
                dbSet.Add(entity);
            else
                dbSet.Where(predicate).UpdateFromQuery(x=> entity);
        }
    }
}
