using System;
using System.Collections.Generic;
using System.Text;

namespace doob.PgSql.Helper
{
    internal static class ExceptionHelper
    {
        public static bool IgnoreErrors(Action operation)
        {
            if (operation == null)
                return false;
            try
            {
                operation.Invoke();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static T IgnoreErrors<T>(Func<T> operation, T defaultValue = default(T))
        {
            if (operation == null)
                return defaultValue;

            T result;
            try
            {
                result = operation.Invoke();
            }
            catch
            {
                result = defaultValue;
            }

            return result;
        }
    }
}
