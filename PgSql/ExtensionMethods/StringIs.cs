using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace doob.PgSql.ExtensionMethods
{
    internal static class StringIs
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrEmpty(value?.Trim());
        }
        public static bool IsNumeric(this string s)
        {
            if (s.IsNullOrWhiteSpace())
                return false;

            bool firstchar = true;
            foreach (char c in s)
            {
                if (firstchar)
                {
                    if (!char.IsDigit(c) && c != '.' && c != '-' && c != '+')
                    {
                        return false;
                    }
                }
                else
                {
                    if (!char.IsDigit(c) && c != '.')
                    {
                        return false;
                    }
                }
                firstchar = false;

            }

            return true;
        }

        public static bool IsInt(this string str)
        {
            if(!IsNumeric(str))
                return false;

            int n;
            return int.TryParse(str, out n);

        }

        public static bool IsLong(this string str)
        {
            if (!IsNumeric(str))
                return false;

            long n;
            return long.TryParse(str, out n);

        }

        public static bool IsDouble(this string str)
        {
            if (!IsNumeric(str))
                return false;

            double n;
            return double.TryParse(str, out n);

        }

        
        public static bool IsBoolean(this string str)
        {
            bool ret;
            return bool.TryParse(str, out ret);
        }

        public static bool IsValidIPv4(this string str)
        {
            System.Net.IPAddress ipAddress;
            return System.Net.IPAddress.TryParse(str, out ipAddress);
        }

        public static Boolean IsValidGuid(this string value)
        {
            Guid g;
            return Guid.TryParse(value, out g);
        }

        public static bool IsBase64Encoded(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            try
            {
                var data = Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsLowerCase(this string str)
        {
            return !string.IsNullOrEmpty(str) && !str.Any(char.IsUpper);
        }

        public static bool IsUpperCase(this string str)
        {
            return !string.IsNullOrEmpty(str) && !str.Any(char.IsLower);
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            domainName = idn.GetAscii(domainName);
            return match.Groups[1].Value + domainName;
        }

        public static Boolean IsValidEmailAddress(this string value)
        {
            //Boolean invalid = false;
            if (String.IsNullOrEmpty(value))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                value = Regex.Replace(value, @"(@)(.+)$", DomainMapper);
            }
            catch
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(value,
                        @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                        RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
