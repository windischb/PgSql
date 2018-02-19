using System;

namespace doob.PgSql.Helper
{
    internal static class Parser
    {
        public static string[] ParseCSVLine(string line, char delimiter = ',')
        {
            string[] rv = new string[0];

            int quotCount = 0;
            bool ignoreNext = false;
            bool hadQuot = false;
            int startPos = 0;

            for (int i = 0; i < line.Length; i++)
            {

                var curr = line[i];

                // ignore char
                if (ignoreNext)
                {
                    ignoreNext = false;
                    continue;
                }

                // ignore next char
                if (line[i].Equals('\\'))
                {
                    ignoreNext = true;
                    continue;
                }

                // push quot
                if (line[i].Equals('"'))
                {
                    hadQuot = true;

                    if (quotCount <= 0)
                    {
                        quotCount++;
                    }
                    else
                    {
                        quotCount--;
                    }
                    continue;
                }

                if (line[i].Equals(delimiter))
                {
                    if (quotCount <= 0)
                    {
                        string cell = line.Substring(startPos, (i - startPos));
                        if (hadQuot)
                        {
                            Array.Resize(ref rv, (rv.Length + 1));
                            rv[(rv.Length - 1)] = cell.Substring(1, (cell.Length - 2));
                        }
                        else
                        {
                            Array.Resize(ref rv, (rv.Length + 1));
                            rv[(rv.Length - 1)] = cell;
                        }
                        startPos = (i + 1);
                        hadQuot = false;
                    }
                }
            }

            // last cell
            if (startPos <= line.Length)
            {
                string cell = line.Substring(startPos, (line.Length - startPos));
                if (hadQuot)
                {
                    Array.Resize(ref rv, (rv.Length + 1));
                    rv[(rv.Length - 1)] = cell.Substring(1, (cell.Length - 2));
                }
                else
                {
                    Array.Resize(ref rv, (rv.Length + 1));
                    rv[(rv.Length - 1)] = cell;
                }
            }

            return rv;
        }

    }
}
