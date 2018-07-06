using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using doob.PgSql.QueryGeneration;
using doob.PgSql.Tables;
using Remotion.Linq;

namespace doob.PgSql
{
    public class PgSqlQueryExecutor : IQueryExecutor
    {
        ITable _table;

        public PgSqlQueryExecutor(ITable table)
        {
            _table = table;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            var psqlCommand = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel, _table);

            var pgCommand = new PgSqlCommand();
            pgCommand.Command = psqlCommand.Statement;
            pgCommand.Parameters.AddRange(psqlCommand.Parameters);

            Debug.WriteLine(pgCommand.CommandAsPlainText());

            return new DbExecuter(_table.GetConnectionString()).ExecuteScalar<T>(pgCommand);

        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ?
                ExecuteCollection<T>(queryModel).SingleOrDefault() :
                ExecuteCollection<T>(queryModel).Single();
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var psqlCommand = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel, _table);

            var pgCommand = new PgSqlCommand();
            pgCommand.Command = psqlCommand.Statement;
            pgCommand.Parameters.AddRange(psqlCommand.Parameters);

            Debug.WriteLine(pgCommand.CommandAsPlainText());

            return new DbExecuter(_table.GetConnectionString()).ExecuteReader<T>(pgCommand);

        }
    }
}
