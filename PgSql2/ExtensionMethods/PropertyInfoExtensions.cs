using System;
using System.Reflection;

namespace PgSql2.ExtensionMethods
{
    internal static class PropertyInfoExtensions
    {
        internal static object GetDefaultValue(this PropertyInfo prop)
        {
            if (prop.PropertyType.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(prop.PropertyType);
            return null;
        }
    }
}
