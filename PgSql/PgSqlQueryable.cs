using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using doob.PgSql.Tables;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace doob.PgSql
{
    public class PgSqlQueryable<T> : QueryableBase<T>
    {

        private static IQueryProvider CreateQueryProvider(ITable table)
        {
            return new DefaultQueryProvider(
                typeof(PgSqlQueryable<>),
                QueryParser.CreateDefault(),
                new PgSqlQueryExecutor(table));
        }

        public PgSqlQueryable(ITable table) : base(CreateQueryProvider(table)) { }

        public PgSqlQueryable(IQueryParser queryParser, IQueryExecutor executor) : base(queryParser, executor)
        {
        }

        public PgSqlQueryable(IQueryProvider provider) : base(provider)
        {
        }

        public PgSqlQueryable(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }
    }
}
