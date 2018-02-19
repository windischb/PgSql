using System;

namespace doob.PgSql.ExtensionMethods
{
    internal static class StringTo
    {
        public static String ToNull(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }
            return value;
        }
        public static int ToInt(this string str, bool throwOnError = false)
        {
            if (!str.IsInt())
            {
                if (throwOnError)
                    throw new InvalidCastException($"Can't cast '{str}' to 'Integer'");
                return default(int);
            }
                
            return int.Parse(str);
        }
        public static int? ToNullableInt(this string value)
        {
            if (value == null)
                return null;

            int i;
            var result = int.TryParse(value, out i);
            if (result)
            {
                return i;
            }
            return null;
        }

        public static long ToLong(this string str)
        {
            if (!str.IsLong())
                return default(long);

            return long.Parse(str);

        }

        public static double ToDouble(this string str)
        {
            if (!str.IsDouble())
                return default(double);

            return double.Parse(str);

        }

        public static bool ToBoolean(this string str)
        {
            if (!str.IsBoolean())
                return default(bool);

            return Boolean.Parse(str);
        }

        public static string ToFormat(this string stringFormat, params object[] args)
        {
            return String.Format(stringFormat, args);
        }



        public static Guid ToGuid(this string value)
        {
            value = value.Trim("\"".ToCharArray());
            
            Guid g;
            if (Guid.TryParse(value, out g))
            {
                return g;
            }
            if (Guid.TryParseExact(value, "N", out g))
            {
                return g;
            }
            return new Guid();
        }

        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                char c;
                c = char.ToLowerInvariant(chars[i]);

                chars[i] = c;
            }

            return new string(chars);
        }
    }


    internal static class InternalStringTo
    {
        public static string EncodeToBase64(this string toEncode)
        {
            if (string.IsNullOrEmpty(toEncode))
                return null;

            var toEncodeAsBytes = System.Text.Encoding.ASCII.GetBytes(toEncode);
            var returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string DecodeFromBase64(this string encodedData)
        {
            if (string.IsNullOrEmpty(encodedData))
                return null;

            var encodedDataAsBytes = Convert.FromBase64String(encodedData);
            var returnValue = System.Text.Encoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}
