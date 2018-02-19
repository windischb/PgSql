using System;
using System.Text.RegularExpressions;

namespace doob.PgSql.Helper
{

    internal class Wildcard : Regex
    {
        public Wildcard(string pattern)
            : base(WildcardToRegex(pattern))
        {
        }

        public Wildcard(string pattern, RegexOptions options)
            : base(WildcardToRegex(pattern), options)
        {
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Escape(pattern).
             Replace("\\*", ".*").
             Replace("\\?", ".") + "$";
        }

        public static Boolean IsMatch(String searchIn, String matchString, Boolean invert = false)
        {
            if (searchIn == null)
                searchIn = "";

            if (matchString == "*" && String.IsNullOrEmpty(searchIn))
                return false;

            var rege = new Wildcard(matchString, RegexOptions.IgnoreCase).IsMatch(searchIn);

            if (invert)
                return !rege;

            return rege;
        }
    }
}
