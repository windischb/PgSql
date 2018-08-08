using Npgsql;
using Npgsql.TypeHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace doob.PgSql.TypeMapping
{
    public class PgSqlTreeHandlerFactory : NpgsqlTypeHandlerFactory<string>
    {
        protected override NpgsqlTypeHandler<string> Create(NpgsqlConnection conn) => new PgSqlTreeHandler(conn);
    }

    class PgSqlTreeHandler : Npgsql.TypeHandlers.TextHandler
    {
        protected internal PgSqlTreeHandler(NpgsqlConnection connection) : base(connection)
        {
        }
    }
}
