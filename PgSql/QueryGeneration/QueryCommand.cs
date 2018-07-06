using System.Collections.Generic;

namespace doob.PgSql.QueryGeneration
{
    /// An anemic class that holds the generated query statement and its parameters that are being passed to the DbConnection.
    public class QueryCommand
    {
        public string Statement { get; }
        public List<PgSqlParameter> Parameters { get; }
        
        public QueryCommand(string statement, List<PgSqlParameter> parameters)
        {
            Statement = statement;
            Parameters = parameters;
        }
    }
}