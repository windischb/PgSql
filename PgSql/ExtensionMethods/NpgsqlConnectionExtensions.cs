using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace doob.PgSql.ExtensionMethods
{
    internal static class NpgsqlConnectionExtensions
    {
        internal static async Task<NpgsqlConnection> OpenConnectionAsync(this NpgsqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));


            switch (connection.State)
            {
                case ConnectionState.Open:
                case ConnectionState.Connecting:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    {
                        break;
                    }
                case ConnectionState.Broken:
                    {
                        connection.Close();
                        await connection.OpenAsync();
                        break;
                    }
                case ConnectionState.Closed:
                    {
                        await connection.OpenAsync();
                        break;
                    }

            }
            return connection;
        }


        internal static NpgsqlConnection OpenConnection(this NpgsqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            
            switch (connection.State)
            {
                case ConnectionState.Open:
                case ConnectionState.Connecting:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    {
                        break;
                    }
                case ConnectionState.Broken:
                    {
                        connection.Close();
                        connection.Open();
                        break;
                    }
                case ConnectionState.Closed:
                    {
                        connection.Open();
                        break;
                    }
               
            }
            return connection;
        }

       
        internal static NpgsqlConnection CloseConnection(this NpgsqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            switch (connection.FullState)
            {
                case ConnectionState.Open:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    {
                        connection.Close();
                        break;
                    }
                case ConnectionState.Broken:
                    {
                        connection.Close();
                        break;
                    }
                case ConnectionState.Closed:
                    {
                        break;
                    }
                case ConnectionState.Connecting:
                {
                    while (connection.FullState == ConnectionState.Connecting)
                    {
                        Thread.Sleep(100);
                    }
                    connection.Close();
                    break;
                }

            }
            return connection;
        }

    }
}
