using System;
using System.Linq;
using System.Reflection;

namespace IqSoft.CP.Common.Attributes
{
    public class PropertyCustomTypeAttribute : Attribute
    {
        public string TypeName { get; set; }
    }

    public class NotExcelPropertyAttribute : Attribute
    {
    }

    public static class TypeExtensions
    {
        public static PropertyInfo[] GetFilteredProperties(this Type type)
        {
            return type.GetProperties().Where(pi => pi.GetCustomAttributes(typeof(NotExcelPropertyAttribute), true).Length == 0).ToArray();
        }
    }
}
