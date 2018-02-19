using System;
using System.Reflection;

namespace doob.PgSql.ExtensionMethods
{
    internal static class ReflectionExtensions
    {

        public static T GetPropertyValue<T>(this object @object, string fieldName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) {
            var type = @object.GetType();

            var property = GetProperty(type, fieldName, bindingFlags);
            var value = property.GetValue(@object);
            return (T) value;
        }

        public static T GetPropertyValue<T>(this Type type, string fieldName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var property = type.GetProperty(fieldName, bindingFlags);
            var value = property.GetValue(type);
            return (T)value;
        }

        public static PropertyInfo GetProperty(Type t, string name, BindingFlags flags)
        {

            var field = t.GetProperty(name, flags);
            if (field != null)
                return field;

            if (t.BaseType != null)
                return GetProperty(t.BaseType, name, flags);

            return null;

        }


        public static T InvokeMethod<T>(this object @object, string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) {

            return InvokeMethod<T>(@object, methodName, new object[0]);
        }

        public static T InvokeMethod<T>(this object @object, string methodName, object parameter, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {

            return (T)@object.GetType().GetMethod(methodName, bindingFlags).Invoke(@object, new object[]{parameter});
        }

        public static T InvokeMethod<T>(this object @object, string methodName, object[] parameters, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) {

            return (T)@object.GetType().GetMethod(methodName, bindingFlags).Invoke(@object, parameters);
        }
    }
}
