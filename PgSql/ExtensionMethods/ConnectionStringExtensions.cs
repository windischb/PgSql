using System;
using System.Collections.Generic;
using System.Linq;

namespace doob.PgSql.ExtensionMethods {

    internal static class ConnectionStringExtensions {

        internal static string ToNpgSqlConnectionString(this ConnectionString connectionString) {
            var dict = new Dictionary<string, string> {
                {"Host", connectionString.ServerName},
            };

            if (connectionString.Port == 0) {
                dict.Add("Port", "5432");
            } else {
                dict.Add("Port", connectionString.Port.ToString());
            }


            if (!String.IsNullOrWhiteSpace(connectionString.UserName))
                dict.Add("Username", connectionString.UserName);

            if (!String.IsNullOrWhiteSpace(connectionString.Password))
                dict.Add("Password", connectionString.Password);

            if (String.IsNullOrWhiteSpace(connectionString.DatabaseName)) {
                dict.Add("Database", "postgres");
            } else {
                dict.Add("Database", connectionString.DatabaseName);
            }

            if (!String.IsNullOrWhiteSpace(connectionString.ApplicationName))
            {
                dict.Add("ApplicationName", connectionString.ApplicationName);
            }

            return String.Join(";", dict.OrderBy(p => p.Key).Select(e => $"{e.Key}={e.Value}"));
        }


        
    }

}