using System;
using System.Linq;
using System.Linq.Expressions;

namespace IqSoft.CP.Common.Models
{
    public class CheckPermissionOutput
    {
        public CheckPermissionOutput()
        {
            HaveAccessForAllObjects = false;
        }

        public bool HaveAccessForAllObjects { get; set; }
        public IQueryable<long> AccessibleObjects { get; set; }
        public IQueryable<string> AccessibleStringObjects { get; set; }
    }

    public class CheckPermissionOutput<T> : CheckPermissionOutput
    {
        public Expression<Func<T, bool>> Filter { get; set; }

        public CheckPermissionOutput<T> Copy()
        {
            return new CheckPermissionOutput<T>
            {
                HaveAccessForAllObjects = HaveAccessForAllObjects,
                AccessibleObjects = AccessibleObjects,
                AccessibleStringObjects = AccessibleStringObjects,
                Filter = Filter
            };
        }
    }
}
