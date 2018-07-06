using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql.Tables;
using Remotion.Linq.Parsing.Structure;

namespace doob.PgSql
{
    public class PgSqlQueryFactory
    {
        public static PgSqlQueryable<T> Queryable<T>(ITable table)
        {
            return new PgSqlQueryable<T>(table);
        }
    }
}
