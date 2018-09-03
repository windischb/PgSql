using System;
using System.Text.RegularExpressions;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql.ExtensionMethods
{
    public static class StringExtensions
    {

        public static string ClearString(this string input)
        {
            return input.TrimToNull(" ", "\"", ".");
        }

        internal static string ReplaceUnescaped(this string value, string search, string replacewith)
        {
            var escapedSearch = Regex.Escape(search);
            var regEx = new Regex($"(?<!\\\\){escapedSearch}");
            return regEx.Replace(value, replacewith);
        }

        public static string EnsureEndsWith(this string source, string end)
        {
            if (source.EndsWith(end))
                return source;

            return $"{source}{end}";
        }

    }
}
