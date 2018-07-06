using System;

namespace doob.PgSql.ExtensionMethods
{
    public static class ActionExtensions
    {
        public static T InvokeAction<T>(this Action<T> action, T instance = default(T)) {
            if (instance == null) {
                instance = Activator.CreateInstance<T>();
            }

            action(instance);
            return instance;
        }

    }
}
